﻿namespace Upgradier.Core;

public interface IEncryptor
{
    byte[] Encrypt(byte[] data);

    byte[] Decrypt(byte[] data);
}