// Робить мінімум роботи: старт рантайму, один вивід у консоль, вихід.
// Увесь виміряний час — це старт процесу й рантайму, а не логіка програми.
Console.WriteLine($"Hello from {(System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported ? "JIT" : "NativeAOT")}");
