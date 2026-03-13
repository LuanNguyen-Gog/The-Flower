#r "nuget: BCrypt.Net-Next"
using System;
Console.WriteLine(BCrypt.Net.BCrypt.HashPassword("admin123"));
Console.WriteLine(BCrypt.Net.BCrypt.HashPassword("customer123"));