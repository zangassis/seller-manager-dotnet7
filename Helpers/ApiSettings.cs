﻿using System.Text;

namespace SellerManager.Helpers;

internal static class ApiSettings
{
    internal static string SecretKey = "6ceccd7405ef4b00b2630009be568cfa";
    internal static byte[] GenerateSecretByte() =>
        Encoding.ASCII.GetBytes(SecretKey);
}
