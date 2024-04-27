using System;
using System.Security.Cryptography;
using System.Text;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;

namespace ProtoFlux.Runtimes.Execution.Nodes.Obsidian.Strings
{
    public enum HashFunction
    {
        MD5,
        SHA1,
        SHA256,
        SHA384,
        SHA512
    }

    [NodeCategory("Obsidian/String")]
    public class HMACNode : ObjectFunctionNode<FrooxEngineContext, string>
    {
        public readonly ObjectInput<string> Message;
        public readonly ObjectInput<string> Key;
        public readonly ObjectInput<HashFunction> HashAlgorithm;

        protected override string Compute(FrooxEngineContext context)
        {
            string message = Message.Evaluate(context);
            string key = Key.Evaluate(context);
            HashFunction hashFunction = HashAlgorithm.Evaluate(context);

            if (string.IsNullOrEmpty(message) || string.IsNullOrEmpty(key))
                return "";

            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);

            using (HMAC hmac = GetHMAC(hashFunction, keyBytes))
            {
                byte[] hash = hmac.ComputeHash(messageBytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

        private HMAC GetHMAC(HashFunction hashFunction, byte[] key)
        {
            switch (hashFunction)
            {
                case HashFunction.MD5:
                    return new HMACMD5(key);
                case HashFunction.SHA1:
                    return new HMACSHA1(key);
                case HashFunction.SHA256:
                    return new HMACSHA256(key);
                case HashFunction.SHA384:
                    return new HMACSHA384(key);
                case HashFunction.SHA512:
                    return new HMACSHA512(key);
                default:
                    throw new ArgumentException($"Unsupported hash function {hashFunction}");
            }
        }
    }
}
