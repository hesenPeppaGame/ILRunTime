using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace ILRunTime
{
    class Program
    {
        static void Main(string[] args)
        {
            var newType = EmitType();
            object newInstance = Activator.CreateInstance(newType, "豆豆", 1);
            newType.InvokeMember("LogToConsole", BindingFlags.InvokeMethod, null, newInstance, null);
            Console.WriteLine(newType.ToString());
            Console.ReadLine();
        }

        private static Type EmitType()
        {
            Type[] wlParams = new Type[] { typeof(string), typeof(object), typeof(object) };
            MethodInfo writeLineMI = typeof(Console).GetMethod("WriteLine", wlParams);

            AssemblyName assemblyName = new AssemblyName();
            assemblyName.Name = "XLuaCodeEmit";

            //创建AssemblyBuilder
            AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave);

            //创建动态程序集的模块
            ModuleBuilder codeEmitModule = assemblyBuilder.DefineDynamicModule("XLuaCodeEmit", "HelloIL.dll");

            //创建一个自定义类型
            TypeBuilder wrapTypeBuilder = codeEmitModule.DefineType("PlayerData", TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed);

            //PlayerData类有这几个字段
            FieldBuilder feildName = wrapTypeBuilder.DefineField("Name", typeof(string), FieldAttributes.Private);
            FieldBuilder feildAge = wrapTypeBuilder.DefineField("Age", typeof(int), FieldAttributes.Private);

            //PlayerData需要构造函数
            ConstructorBuilder constructorBuilder = wrapTypeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[] { typeof(string), typeof(int) });
            ILGenerator ilOfCtor = constructorBuilder.GetILGenerator();

            //执行System.Object的默认构造函数
            ConstructorInfo constructor = typeof(System.Object).GetConstructor(new Type[0]);
            ilOfCtor.Emit(OpCodes.Ldarg_0);//this
            ilOfCtor.Emit(OpCodes.Call, constructor);

            //执行PlayerData的构造函数，有两个参数，一个Name，一个Age
            ilOfCtor.Emit(OpCodes.Ldarg_0);//this
            ilOfCtor.Emit(OpCodes.Ldarg_1);
            //(字段) static readonly OpCode OpCodes.Stfld用新值替换在对象引用或指针的字段中存储的值。
            ilOfCtor.Emit(OpCodes.Stfld, feildName);

            ilOfCtor.Emit(OpCodes.Ldarg_0);//this
            ilOfCtor.Emit(OpCodes.Ldarg_2);
            //(字段) static readonly OpCode OpCodes.Stfld用新值替换在对象引用或指针的字段中存储的值。
            ilOfCtor.Emit(OpCodes.Stfld, feildAge);

            ilOfCtor.Emit(OpCodes.Ret);

            //添加第一个方法
            MethodBuilder methodBuilder = wrapTypeBuilder.DefineMethod("LogToConsole", MethodAttributes.Public, null, null);
            ILGenerator il = methodBuilder.GetILGenerator();
            il.Emit(OpCodes.Ldstr, "{0},{1}");
            il.Emit(OpCodes.Ldstr, "我爱");
            il.Emit(OpCodes.Ldstr, "豆豆");
            //执行方法
            il.EmitCall(OpCodes.Call, writeLineMI, null);
            //返回
            il.Emit(OpCodes.Ret);

            //添加Tostring方法
            MethodBuilder toStringMethodInfo = wrapTypeBuilder.DefineMethod("ToString", MethodAttributes.Virtual | MethodAttributes.Public, typeof(string), null);
            ILGenerator toStringIL = toStringMethodInfo.GetILGenerator();
            //创建本地变量
            LocalBuilder localVar = toStringIL.DeclareLocal(typeof(string));
            toStringIL.Emit(System.Reflection.Emit.OpCodes.Ldstr, "我的名字{0},今年{1}岁了");
            //将Name放到计算堆栈上
            toStringIL.Emit(OpCodes.Ldarg_0);//this
            toStringIL.Emit(OpCodes.Ldfld, feildName);
            //将Age放到计算堆栈上
            toStringIL.Emit(OpCodes.Ldarg_0);//this
            toStringIL.Emit(OpCodes.Ldfld, feildAge);
            toStringIL.Emit(OpCodes.Box, typeof(int));
            //调用String.Format方法
            MethodInfo stringFormatMethod = typeof(string).GetMethod("Format", new Type[] { typeof(string), typeof(object), typeof(object) });
            toStringIL.Emit(OpCodes.Call, stringFormatMethod);
            //从计算堆栈的顶部单出当前值并将其存储到指定索引处的局部变量列表中。
            toStringIL.Emit(OpCodes.Stloc, localVar);
            //将指定索引处的局部变量加载到计算堆栈上。
            toStringIL.Emit(OpCodes.Ldloc, localVar);
            toStringIL.Emit(OpCodes.Ret);

            //创建类型
            Type classType = wrapTypeBuilder.CreateType();

            //保存到本地
            assemblyBuilder.Save("HelloIL.dll");

            return classType;
        }

    }
}
