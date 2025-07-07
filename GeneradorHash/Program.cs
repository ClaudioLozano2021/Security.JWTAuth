using JWTAuth.Helpers;

JWTPrivateKeyGenerator generator = new JWTPrivateKeyGenerator();

Console.Write("Ingrese el texto con el cual quiere armar el Hash: ");
string hash = Console.ReadLine()!;

hash = generator.GenerarClavePrivada512(hash);
Console.WriteLine("Presione una tecla");
Console.ReadKey();
Console.Clear();

Console.WriteLine($"Su codigo Hash es: {hash}");
Console.WriteLine("Presione una tecla para salir (previamente copie el hash)");
Console.ReadKey();
Console.Clear();
Environment.Exit(0);
