const Scheme = require('./Scheme.js');
const crypto = require('crypto');

const notRandom = (len) => {
    const bytes = [];
    for (let i = 0; i < len; i++) {
      bytes[i] = i + 1;
    }
    return bytes;
}

// given hex string, return json
const split = (isRandom, n, k, secret) => {
    // secret is hex string encoded, e.g. 1234abcd 
    // n and k is number.
    const u8array = new Uint8Array(Buffer.from(secret, "hex"));
    const result = Scheme.split(isRandom === "true" ? crypto.randomBytes: notRandom, +n, +k, u8array);
    const resultObject = {};
    Object.keys(result).forEach((key) => {
        const value = result[key];
        resultObject[""+key] = Buffer.from(value).toString('hex');
    })
    return JSON.stringify(resultObject);
};

const join = (jsonStr) => {
    const splitObject = JSON.parse(jsonStr);
    const resultObject = {};
    Object.keys(splitObject).forEach((key) => {
        const value = splitObject[key];
        resultObject[""+key] = new Uint8Array(Buffer.from(value, 'hex'))
    });
    const joined = Scheme.join(resultObject);
    return Buffer.from(joined).toString('hex');
};

const args = process.argv;
if (args[2] === "split"){ // split
    // 3, 2, 1234abcd
    console.log(split(args[3], args[4], args[5], args[6]));
}else{ // join
    // '{"1": "ab5678", "2", "abcdef", "3": "999999"}'
    const result = join(args[3]);
    console.log(result);
}