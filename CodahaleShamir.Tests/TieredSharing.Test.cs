using Xunit;
using CodahaleShamir.Scheme;
using System.Collections.Generic;
using System;

namespace CodahaleShamir.Tests
{
    public class TieredSharingTest
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
        public void TieredSharingTestsRoundtrip()
        {
            ShamirScheme scheme = new ShamirScheme();

            string secretUtf8 = "ᚠᛇᚻ";
            byte[] secret = stringToBytes(secretUtf8);
            byte adminParts = 3;
            byte adminQuorum = 2;
            Dictionary<string, byte[]> adminSplits = scheme.split(randomBytes, adminParts, adminQuorum, secret);
            byte userParts = 4;
            byte userQuorum = 3;
            Dictionary<string, byte[]> userSplits = scheme.split(randomBytes, userParts, userQuorum, adminSplits["3"]);

            // throw away third share that is split into 4 user parts
            adminSplits.Remove("3");
            byte[] joinedAdminShares = scheme.join(adminSplits);
            Assert.Equal(secretUtf8, bytesToString(joinedAdminShares));

            // throw away second admin share we only have one remaining
            adminSplits.Remove("2");
            // throw away one user share as we only need three
            userSplits.Remove("1");

            // reconstruct the third admin share from the three user shares 
            byte[] joinedUserShares = scheme.join(userSplits);

            // use the first admin share and the recovered third share 
            Dictionary<string, byte[]> mixedShares = new Dictionary<string, byte[]>();
            mixedShares.Add("1", adminSplits["1"]);
            mixedShares.Add("3", joinedUserShares);

            byte[] joinedMixedShares = scheme.join(mixedShares);
            Assert.Equal(secretUtf8, bytesToString(joinedMixedShares));
        }
    }
}