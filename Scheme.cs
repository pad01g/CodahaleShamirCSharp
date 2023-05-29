using GF256;

namespace Scheme;
public class Scheme
{
    public Field gf256;
    public Scheme(){
        gf256 = new Field();
    }
    /**
    * Splits the given secret into {@code n} parts, of which any {@code k} or more can be combined to
    * recover the original secret.
    * @param  {function int -> Uint8Array} randomBytes Takes a length and returns a random
    * Uint8Array of that length
    * @param  {Number} n the number of parts to produce (must be {@code >1})
    * @param  {Number} k the threshold of joinable parts (must be {@code <= n})
    * @param  {array[Uint8Array]} secret The secret to split as an array of bytes
    * @return {Object.<string, Uint8Array>} an map of {@code n} parts that are arrays of bytes of the
    * secret length
    */
    public uint[][] split(Func<uint, uint[]> randomBytes, uint n, uint k, uint[] secret) {
        if (k <= 1) throw new Exception("K must be > 1");
        if (n < k) throw new Exception("N must be >= K");
        if (n > 255) throw new Exception("N must be <= 255");

        uint[][] values = new uint[n][];
        for(int i=0; i<(uint)values.Length; i++ ){
            values[i] = new uint[(uint)secret.Length];
        }

        // eslint-disable-next-line no-plusplus
        for (uint i = 0; i < (uint)secret.Length; i++) {
            uint[] p = gf256.generate(randomBytes, k - 1, secret[i]);
            // eslint-disable-next-line no-plusplus
            for (uint x = 1; x <= n; x++) {
                values[x - 1][i] = gf256.eval(p, x);
            }
        }

        uint[][] parts = new uint[(uint)values.Length][];

        // this is zero-index, not 1-index of original
        for (uint i = 0; i < (uint)values.Length; i++) {
            parts[i] = values[i];
        }

        return parts;
    }


    /**
    * Joins the given parts to recover the original secret.
    *
    * <p><b>N.B.:</b> There is no way to determine whether or not the returned value is actually the
    * original secret. If the parts are incorrect, or are under the threshold value used to split the
    * secret, a random value will be returned.
    *
    * @param {Object.<string, Uint8Array>} parts an map of {@code n} parts that are arrays of bytes
    * of the secret length
    * @return {Uint8Array} the original secret
    *
    */
    public uint[] join(uint[][] parts) {
        if (parts.Length == 0) throw new Exception("No parts provided");
        uint[] lengths = new uint[(uint)parts.Length]; // Object.values(parts).map(x => x.length);
        for (uint i = 0; i < (uint)parts.Length; i++){
            lengths[i] = (uint)parts[i].Length;
        }
        uint max = lengths.Max();
        uint min = lengths.Min();
        if (max != min) {
            throw new Exception($"Varying lengths of part values. Min {min}, Max {max}");
        }
        uint[] secret = new uint[max];
        // eslint-disable-next-line no-plusplus
        for (uint i = 0; i < (uint)secret.Length; i++) {
            uint[] keys = new uint[(uint)parts.Length];
            for(uint j = 0; j < parts.Length; j++){
                keys[j] = j;
            }
            uint[][] points = new uint[keys.Length][];
            for(uint j = 0; j < points.Length; j++){
                points[j] = new uint[]{0, 0};
            }
            
            // eslint-disable-next-line no-plusplus
            for (uint j = 0; j < keys.Length; j++) {
                uint key = keys[j];
                uint k = key + 1;
                points[j][0] = k;
                points[j][1] = parts[key][i];
            }
            secret[i] = gf256.interpolate(points);
        }

        return secret;
    }
}