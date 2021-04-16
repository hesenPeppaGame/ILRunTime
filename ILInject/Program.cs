using System;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ILInject
{
    /// <summary>
    /// 使用Mono.Cecil实现IL的注入
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            ILInjection();
        }

        /// <summary>
        /// 注意 IL注入时，符号表.pbd文件一定要与dll在同一目录，否则会报错
        /// </summary>
        private static void ILInjection()
        {
            string path = "D://ClassLibrary1.dll";
            var readerParameters = new ReaderParameters { ReadSymbols = true };
            AssemblyDefinition assemblyDefinition = Mono.Cecil.AssemblyDefinition.ReadAssembly(path, readerParameters);
            ModuleDefinition mainModule = assemblyDefinition.MainModule;
            try
            {
                foreach (TypeDefinition type in mainModule.GetTypes())
                {
                    if (type.Name == "DebugClass")
                    {
                        Mono.Collections.Generic.Collection<MethodDefinition> methods = type.Methods;
                        foreach (MethodDefinition method in methods)
                        {
                            if (method.Name == "ConsoleLog")
                            {
                                // 开始注入IL代码
                                var insertPoint = method.Body.Instructions[0];
                                var ilProcessor = method.Body.GetILProcessor();
                                ilProcessor.InsertBefore(insertPoint, ilProcessor.Create(OpCodes.Nop));
                                ilProcessor.InsertBefore(insertPoint, ilProcessor.Create(OpCodes.Ldstr, "a = {0}, b = {1}"));
                                ilProcessor.InsertBefore(insertPoint, ilProcessor.Create(OpCodes.Ret));
                            }
                        }
                    }
                }
            }
            finally
            {
                if (assemblyDefinition.MainModule.SymbolReader != null)
                {
                    assemblyDefinition.MainModule.SymbolReader.Dispose();
                }

                assemblyDefinition.Write("D://ClassLibrary2.dll", new WriterParameters() { WriteSymbols = true });
            }
        }
    }
}
