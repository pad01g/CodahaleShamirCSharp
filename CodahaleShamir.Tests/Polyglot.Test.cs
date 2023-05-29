using Xunit;
using CodahaleShamir.Scheme;
using System.Collections.Generic;
using System;
using JavaScheme = com.codahale.shamir.Scheme;
using JavaSecureRandom = java.security.SecureRandom;

namespace CodahaleShamir.Tests
{
    public class PolyglotTest
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
            // Scheme is for java, ShamirScheme is for csharp.
            ShamirScheme scheme = new ShamirScheme();
            // this should work with right platform. currently SecureRandom is not working on ubuntu 22.04
            // so skip this test
            JavaScheme javaScheme = new JavaScheme(new JavaSecureRandom(), 5, 3);
        }
    }
}