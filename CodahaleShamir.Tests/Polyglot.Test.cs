using Xunit;
using CodahaleShamir.Scheme;
using System.Collections.Generic;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Diagnostics;
using System.Linq;

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

        public byte[] notRandomBytes(uint size) {
            byte[] p = new byte[size];
            for (int i = 0; i < size; i++){
                p[i] = (byte) (i + 1);
            }
            return p;
        }

        public static string bytesToString(byte[] bytes){
            return System.Text.Encoding.Unicode.GetString(bytes);
        }

        public static byte[] stringToBytes(string str){
            return System.Text.Encoding.Unicode.GetBytes(str);
        }

        public static byte[] hexStringToBytes(string hex) {
            return Enumerable.Range(0, hex.Length)
                            .Where(x => x % 2 == 0)
                            .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                            .ToArray();
        }

        public static bool equalParts(Dictionary<string, byte[]> jsParts, Dictionary<string, byte[]> csParts) {
            if(jsParts.Count != csParts.Count){
                return false;
            }
            foreach (KeyValuePair<string, byte[]> jsKvp in jsParts) {
                string key = jsKvp.Key;
                byte[] jsValue = jsKvp.Value;
                byte[] csValue = csParts[key];
                if (jsValue.Length != csValue.Length){
                    return false;
                }
                for(int i = 0; i < jsValue.Length; i++){
                    if(jsValue[i] != csValue[i]){
                        return false;
                    }
                }
            }
            return true;
        }

        public static Dictionary<string, byte[]> javascriptSplit(bool isRandom, byte n, byte k, byte[] secret){
            // hex string
            string secretString = BitConverter.ToString(secret).Replace("-", string.Empty);
            string randomness = isRandom?"true":"false";
            ProcessStartInfo psi = new ProcessStartInfo("node", $"../../../ext/cli.js split {randomness} {n} {k} {secretString}");
            psi.RedirectStandardOutput = true;
            Process p = Process.Start(psi);

            // json is returned
            string output = p.StandardOutput.ReadToEnd();
    
            p.WaitForExit();
            p.Close();

            Dictionary<string, string>? json = JsonSerializer.Deserialize<Dictionary<string, string>>(output);
            if (json == null){
                throw new Exception("not possible to get split output");
            }else{
                Dictionary<string, byte[]> result = new Dictionary<string, byte[]>();
                foreach (KeyValuePair<string, string> kvp in json) {
                    string key = kvp.Key;
                    string hexValue = kvp.Value;
                    byte[] bytes = hexStringToBytes(hexValue);
                    result.Add(key, bytes);
                }
                return result;
            }
        }

        public static byte[] javascriptJoin(Dictionary<string, byte[]> parts){
            // convert dictionary to json string
            Dictionary<string, string> hexDict = new Dictionary<string, string>();
            foreach (KeyValuePair<string, byte[]> kvp in parts) {
                string key = kvp.Key;
                string hexValue = BitConverter.ToString(kvp.Value).Replace("-", string.Empty);
                hexDict.Add(key, hexValue);
            }

            string partsJson = JsonSerializer.Serialize(hexDict).Replace("\"", "\\\"");
            ProcessStartInfo psi = new ProcessStartInfo("node", $"../../../ext/cli.js join {partsJson} ");
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            Process p = Process.Start(psi);

            // json is returned
            string output = p.StandardOutput.ReadToEnd();
    
            p.WaitForExit();
            p.Close();

            // output is hex, so convert it to bytes and return.
            return hexStringToBytes(output.Trim());
        }

        [Fact]
        public void PolygotTestsCompareJavascriptAndCsharpSplit()
        {
            ShamirScheme scheme = new ShamirScheme();
            string secretUtf8 = "ᚠᛇᚻ";
            byte[] secret = stringToBytes(secretUtf8);
            Dictionary<string, byte[]> jsResult = javascriptSplit(false, 3, 2, secret);
            Dictionary<string, byte[]> csResult = scheme.split(notRandomBytes, 3, 2, secret);
            Assert.True(equalParts(jsResult, csResult));
        }

        [Fact]
        public void PolygotTestsJavascriptSplitWithCsharpJoin()
        {
            ShamirScheme scheme = new ShamirScheme();
            string secretUtf8 = "ᚠᛇᚻ";
            byte[] secret = stringToBytes(secretUtf8);
            Dictionary<string, byte[]> jsResult = javascriptSplit(true, 3, 2, secret);
            byte[] recovered = scheme.join(jsResult);
            Assert.Equal(bytesToString(recovered), secretUtf8);
        }

        [Fact]
        public void PolygotTestsCsharpSplitWithJavascriptJoin()
        {
            ShamirScheme scheme = new ShamirScheme();
            string secretUtf8 = "ᚠᛇᚻ";
            byte[] secret = stringToBytes(secretUtf8);
            Dictionary<string, byte[]> csResult = scheme.split(notRandomBytes, 3, 2, secret);
            byte[] recovered = javascriptJoin(csResult);
            Assert.Equal(bytesToString(recovered), secretUtf8);
        }
    }
}