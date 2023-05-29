using Xunit;
// using CodahaleShamir;
using CodahaleShamir.Scheme;
using System.Collections.Generic;
using System;
using System.Text;

namespace CodahaleShamir.Tests
{

    public class SchemeTest
    {
        public byte[] randomBytes(uint size) {
            byte[] p = new byte[size];
            Random rnd = new Random();
            rnd.NextBytes(p);
            return p;
        }

        public static string bytesToString(byte[] bytes){
            return System.Text.Encoding.Unicode.GetString(bytes);
        }

        public static byte[] stringToBytes(string str){
            return System.Text.Encoding.Unicode.GetBytes(str);
        }

        [Fact]
        public void SchemeTestsRoundtrip()
        {
            ShamirScheme scheme = new ShamirScheme();
            byte parts = 5;
            byte quorum = 3;
            string secretUtf8 = @"ᚠᛇᚻ᛫ᛒᛦᚦ᛫ᚠᚱᚩᚠᚢᚱ᛫ᚠᛁᚱᚪ᛫ᚷᛖᚻᚹᛦᛚᚳᚢᛗ
ᛋᚳᛖᚪᛚ᛫ᚦᛖᚪᚻ᛫ᛗᚪᚾᚾᚪ᛫ᚷᛖᚻᚹᛦᛚᚳ᛫ᛗᛁᚳᛚᚢᚾ᛫ᚻᛦᛏ᛫ᛞᚫᛚᚪᚾ
ᚷᛁᚠ᛫ᚻᛖ᛫ᚹᛁᛚᛖ᛫ᚠᚩᚱ᛫ᛞᚱᛁᚻᛏᚾᛖ᛫ᛞᚩᛗᛖᛋ᛫ᚻᛚᛇᛏᚪᚾ᛬";

            byte[] secret = stringToBytes(secretUtf8);
            Dictionary<string, byte[]> splits = scheme.split(randomBytes, parts, quorum, secret);
            splits.Remove("2");
            splits.Remove("3");

            byte[] joined = scheme.join(splits);
            Assert.Equal(joined[200], secret[200]);
            Assert.Equal(joined[201], secret[201]);
            string joinedUtf8 = bytesToString(joined);
            Assert.Equal(secretUtf8, joinedUtf8);
        }

        [Fact]
        public void SchemeTestsRoundtripTwoParts()
        {
            ShamirScheme scheme = new ShamirScheme();
            byte parts = 3;
            byte quorum = 2;
            string secretUtf8 = "ᚠᛇᚻ";
            byte[] secret = stringToBytes(secretUtf8);

            for(uint i = 1; i <= 3; i++){
                Dictionary<string, byte[]> splits = scheme.split(randomBytes, parts, quorum, secret);
                splits.Remove(i.ToString());
                string joinedUtf8 = bytesToString(scheme.join(splits));
                Assert.Equal(secretUtf8, joinedUtf8);
            }
        }

        [Fact]
        public void SchemeTestsSplitInputValidation()
        {
            ShamirScheme scheme = new ShamirScheme();
            string secretUtf8 = "ᚠᛇᚻ";
            byte[] secret = stringToBytes(secretUtf8);

            // type check for byte < 256 is not necessary in csharp, skipping test.
            // try {
            //     scheme.split(randomBytes, (byte)256, (byte)2, secret);
            //     Assert.True(false);
            // } catch (Exception e) {
            //     Assert.Contains("N must be <= 255", e.ToString());
            // }

            try {
                scheme.split(randomBytes, (byte)3, (byte)1, secret);
                Assert.True(false);
            } catch (Exception e) {
                Assert.Contains("K must be > 1", e.ToString());
            }

            try {
                scheme.split(randomBytes, (byte)2, (byte)3, secret);
                Assert.True(false);
            } catch (Exception e) {
                Assert.Contains("N must be >= K", e.ToString());
            }
        }

        [Fact]
        public void SchemeTestsJoinInputValidation()
        {
            ShamirScheme scheme = new ShamirScheme();
            Dictionary<string, byte[]> parts = new Dictionary<string, byte[]>();

            try {
                scheme.join(parts);
                Assert.True(false);
            } catch (Exception e) {
                Assert.Contains("No parts provided", e.ToString());
            }

            try {
                Dictionary<string, byte[]> splits = scheme.split(randomBytes, 3, 2, stringToBytes("ᚠᛇᚻ"));
                splits["2"] = new byte[]{216, 30, 190, 102};
                scheme.join(splits);
                Assert.True(false);
            } catch (Exception e) {
                Assert.Contains("Varying lengths of part values", e.ToString());
            }
        }
    }
}