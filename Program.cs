using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Policy;
using System.Linq;

class Test
{
    public static void Main()
    {
        AppDomainSetup domaininfo = new AppDomainSetup();
        domaininfo.ApplicationBase = System.Environment.CurrentDirectory;
        Evidence adevidence = AppDomain.CurrentDomain.Evidence;
        AppDomain currentDomain = AppDomain.CreateDomain("MyDomain", adevidence, domaininfo);

        //E:\Users\RODCHENKO\Documents\GitHub\Seatbelt\Seatbelt\bin\Debug\Seatbelt.dll
        //AppDomain currentDomain = AppDomain.CurrentDomain;

        //InstantiateMyType(currentDomain);   // Failed!

        currentDomain.AssemblyResolve += new ResolveEventHandler(MyResolver);
        //currentDomain.ReflectionOnlyAssemblyResolve += new ResolveEventHandler(MyResolver);
        //currentDomain.TypeResolve += new ResolveEventHandler(MyResolver);

        InstantiateMyType(currentDomain);   // OK!
    }

    static void InstantiateMyType(AppDomain domain)
    {
        try
        {
            // You must supply a valid fully qualified assembly name here.
            domain.CreateInstance("Seatbelt, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "Seatbelt.Program");
            Assembly[] AllAssemblies = domain.GetAssemblies();
            List<Assembly> assemblies = AllAssemblies.OfType<Assembly>().ToList();
            Assembly a = assemblies.Where(n => n.FullName.Contains("Seatbelt")).First();  //.GetEnumerator().(""); _assembly.GetType(typeName);
                                                                                                                                  // TODO: this won't work if there are overloads available
            Type type = a.GetType("Seatbelt.Program");
            MethodInfo method = type.GetMethod(
                "ListUserFolders",
                BindingFlags.Static | BindingFlags.Public);
            method.Invoke(null, null);

            /*
             * Type type = _assembly.GetType(typeName);
                // TODO: this won't work if there are overloads available
                MethodInfo method = type.GetMethod(
                    methodName,
                    BindingFlags.Static | BindingFlags.Public);
                return method.Invoke(null, parameters);
             */

        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    // Loads the content of a file to a byte array. 
    static byte[] loadFile(string filename)
    {
        FileStream fs = new FileStream(filename, FileMode.Open);
        byte[] buffer = new byte[(int)fs.Length];
        fs.Read(buffer, 0, buffer.Length);
        fs.Close();

        return buffer;
    }

    static Assembly MyResolver(object sender, ResolveEventArgs args)
    {
        AppDomain domain = (AppDomain)sender;

        // Once the files are generated, this call is
        // actually no longer necessary.
        //EmitAssembly(domain);

        //byte[] rawAssembly = loadFile(@"E:\Users\RODCHENKO\Documents\GitHub\Seatbelt\Seatbelt\bin\Debug\Seatbelt.dll");
        //byte[] rawSymbolStore = loadFile("temp.pdb");
        //Assembly assembly = domain.Load(rawAssembly);
        Assembly assembly = domain.Load(@"E:\Users\RODCHENKO\Documents\GitHub\Seatbelt\Seatbelt\bin\Debug\Seatbelt.dll");
        AssemblyName [] names = assembly.GetReferencedAssemblies();
        foreach (AssemblyName name in names) {
            domain.Load(name);
        }
        return assembly;
    }

    // Creates a dynamic assembly with symbol information
    // and saves them to temp.dll and temp.pdb
    static void EmitAssembly(AppDomain domain)
    {
        AssemblyName assemblyName = new AssemblyName();
        assemblyName.Name = "MyAssembly";

        AssemblyBuilder assemblyBuilder = domain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Save);
        ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("MyModule", "temp.dll", true);
        TypeBuilder typeBuilder = moduleBuilder.DefineType("MyType", TypeAttributes.Public);

        ConstructorBuilder constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, null);
        ILGenerator ilGenerator = constructorBuilder.GetILGenerator();
        ilGenerator.EmitWriteLine("MyType instantiated!");
        ilGenerator.Emit(OpCodes.Ret);

        typeBuilder.CreateType();

        assemblyBuilder.Save("temp.dll");
    }
}