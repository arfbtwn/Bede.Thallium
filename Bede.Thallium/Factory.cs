﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;

namespace Bede.Thallium
{
    using Belt;
    using Content;

    using Ident     = Tuple<Type, Type>;
    using Param     = KeyValuePair<string, object>;
    using Params    = Dictionary<string, object>;
    using Formatter = MediaTypeFormatter;

    class Factory
    {
        static readonly Type[]                  EmpT     = Type.EmptyTypes;

        static readonly Lazy<ModuleBuilder>     Builder  = new Lazy<ModuleBuilder>(ConstructBuilder);

        static readonly Dictionary<Ident, Type> Built    = new Dictionary<Ident, Type>();

        static readonly AssemblyName            Assembly = new AssemblyName("RestClients");

        static ModuleBuilder ModB
        {
            get { return Builder.Value; }
        }

        static ModuleBuilder ConstructBuilder()
        {
#if DEBUG
            const bool symbols = true;
#else
            const bool symbols = false;
#endif
            return AssemblyBuilder.DefineDynamicAssembly(Assembly, AssemblyBuilderAccess.Run)
                                  .DefineDynamicModule("AutoGenerated", symbols);

        }

        static MethodInfo GetMethod(Type type, string name, Type[] genericTypes, Type[] parameterTypes)
        {
            parameterTypes = parameterTypes ?? EmpT;
            genericTypes   = genericTypes   ?? EmpT;

            var noGeneric = !genericTypes.Any();

            return type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)

                .Where         (x => x.Name == name && (noGeneric ^ x.IsGenericMethodDefinition))
                .Select        (x => MakeDefinition(x, genericTypes))
                .FirstOrDefault(x => x.GetParameters().Select(p => p.ParameterType).SequenceEqual(parameterTypes));
        }

        static MethodInfo MakeDefinition(MethodInfo x, Type[] genericTypes)
        {
            try
            {
                return x.IsGenericMethodDefinition ? x.MakeGenericMethod(genericTypes) : x;
            }
            catch (ArgumentException)
            {
                return x;
            }
        }

        static Type[] SendAsyncParams()
        {
            return new []
            {
                typeof(HttpMethod),
                typeof(string),
                typeof(Params),
                typeof(Params),
                typeof(HttpContent),
                typeof(CancellationToken?)
            };
        }

        static string MakeName(Type parent, Type target)
        {
            var tn = target.Name;
            var crete = 'I' == tn[0] ? tn.Substring(1) : tn;
            return parent.Namespace + "." + crete + parent.Name;
        }

        internal IImp Imp = new Imp();

        internal Type Build<TBase, T>(IIntrospect introspector)
            where TBase : RestClient
        {
            var parent = typeof(TBase);
            var target = typeof(T);

            return Build(parent, target, introspector);
        }

        internal Type Build(Type parent, Type target, IIntrospect introspector)
        {
            var ident  = Tuple.Create(target, parent);

            if (Built.ContainsKey(ident))
            {
                return Built[ident];
            }

            Assertion.IsInterface("target", target);

            var methods = target.GetMethods()
                                .Union(target.GetInterfaces().SelectMany(i => i.GetMethods()))
                                .Where(ReflectionExtensions.IsMethod)
                                .ToArray();

            Assertion.AllAsyncMethods("target", methods);

            var targetName = MakeName(parent, target);

            var typB = ModB.DefineType(name:       targetName,
                                       attr:       TypeAttributes.Class,
                                       parent:     parent,
                                       interfaces: new [] { target });

            foreach (var method in methods)
            {
                var call = introspector.Call(target, method);

                var args = method.GetParameters();

                var metB = typB.DefineMethod(name:              method.DeclaringType.Name + "." + method.Name,
                                             attributes:        method.Attributes & ~MethodAttributes.Abstract,
                                             callingConvention: method.CallingConvention,
                                             returnType:        method.ReturnType,
                                             parameterTypes:    args.Select(x => x.ParameterType).ToArray());

                var ilG = metB.GetILGenerator();

                typB.DefineMethodOverride(metB, method);

                // Separate parameters
                var bodyP = call.Body.Keys.ToArray();
                var headP = call.Headers.Keys.ToArray();
                var restP = args.Except(bodyP.Union(headP)).ToArray();

                var caTok = args.FirstOrDefault(x => typeof(CancellationToken?).IsAssignableFrom(x.ParameterType));

                // Decide immediately if we can tail call from SendAsync
                var thrmT = typeof(Task<HttpResponseMessage>);
                var tail  = thrmT == method.ReturnType;
                var ctHM  = typeof(HttpMethod).GetConstructor(new [] { typeof(string) });

                ilG.Emit(OpCodes.Ldarg_0);
                if (!tail)
                {
                    ilG.Emit(OpCodes.Dup);
                }
                ilG.Emit(OpCodes.Ldstr, call.Verb.Method);
                ilG.Emit(OpCodes.Newobj, ctHM);
                ilG.Emit(OpCodes.Ldstr, call.Template);

                var dict = typeof(Params);
                var ctor = dict.GetConstructor(EmpT);
                var madd = dict.GetProperty("Item").SetMethod;

                // Construct parameters dictionary
                ilG.Emit(OpCodes.Newobj, ctor);
                foreach (var arg in restP)
                {
                    ilG.Emit(OpCodes.Dup);
                    ilG.Emit(OpCodes.Ldstr, arg.Name);
                    ilG.Emit(OpCodes.Ldarg, arg.Position + 1);

                    if (arg.ParameterType.IsValueType)
                    {
                        ilG.Emit(OpCodes.Box, arg.ParameterType);
                    }

                    ilG.Emit(OpCodes.Call, madd);
                }

                // Construct headers dictionary
                ilG.Emit(OpCodes.Newobj, ctor);

                // Pack static headers, but ignore the ones we would replace
                foreach (var head in call.Static)
                {
                    ilG.Emit(OpCodes.Dup);
                    ilG.Emit(OpCodes.Ldstr, head.Key);
                    ilG.Emit(OpCodes.Ldc_I4, head.Value.Length);
                    ilG.Emit(OpCodes.Newarr, typeof(string));

                    for (int i = 0, end = head.Value.Length; i < end ; ++i)
                    {
                        ilG.Emit(OpCodes.Dup);
                        ilG.Emit(OpCodes.Ldc_I4, i);
                        ilG.Emit(OpCodes.Ldstr, head.Value[i]);
                        ilG.Emit(OpCodes.Stelem, typeof(string));
                    }

                    ilG.Emit(OpCodes.Call, madd);
                }

                // Pack parameter headers
                foreach (var head in headP)
                {
                    ilG.Emit(OpCodes.Dup);
                    ilG.Emit(OpCodes.Ldstr, call.Headers[head]);
                    ilG.Emit(OpCodes.Ldarg, head.Position + 1);

                    if (head.ParameterType.IsValueType)
                    {
                        ilG.Emit(OpCodes.Box, head.ParameterType);
                    }

                    ilG.Emit(OpCodes.Call, madd);
                }

                var delM = GetMethod(parent, "SendAsync", null, SendAsyncParams());
                // Build content
                if (0 == bodyP.Length)
                {
                    ilG.Emit(OpCodes.Ldnull);
                }
                else
                {
                    var imp = Imp;

                    var ctb = typeof(RestClient).GetMethod("ContentBuilder", BindingFlags.NonPublic | BindingFlags.Instance);
                    ilG.Emit(OpCodes.Ldarg_0);
                    ilG.Emit(OpCodes.Callvirt, ctb);

                    imp.Process(ilG, call);
                }

                // Load cancellation token
                if (caTok != null)
                {
                    ilG.Emit(OpCodes.Ldarg, caTok.Position + 1);

                    ilG.EmitNullConversion(caTok.ParameterType);
                }
                else
                {
                    ilG.Emit(OpCodes.Ldnull);
                    ilG.Emit(OpCodes.Castclass, typeof(CancellationToken?));
                }

                // Call SendAsync
                if (tail)
                {
                    ilG.Emit(OpCodes.Tailcall);
                }
                ilG.EmitCall(delM);

                // Convert return type if necessary
                if (!tail)
                {
                    var conM = method.ReturnType.IsGenericType
                        ? GetMethod(parent, "Return", method.ReturnType.GetGenericArguments(), new [] { thrmT })
                        : GetMethod(parent, "Return", null,                                    new [] { thrmT });

                    ilG.Emit(OpCodes.Tailcall);
                    ilG.EmitCall(conM);
                }
                ilG.Emit(OpCodes.Ret);
            }

            // Generate constructors
            foreach (var ctor in parent.GetConstructors(BindingFlags.Public | BindingFlags.Instance))
            {
                var ctorP = ctor.GetParameters();
                var ctorB = typB.DefineConstructor(MethodAttributes.Public,
                                                   CallingConventions.Standard | CallingConventions.HasThis,
                                                   ctorP.Select(x => x.ParameterType).ToArray());
                var ilG   = ctorB.GetILGenerator();
                ilG.Emit(OpCodes.Ldarg_0);
                foreach (var param in ctorP)
                {
                    var p = ctorB.DefineParameter(param.Position + 1, param.Attributes, param.Name);
                    ilG.Emit(OpCodes.Ldarg, p.Position);
                }
                ilG.Emit(OpCodes.Call, ctor);
                ilG.Emit(OpCodes.Ret);
            }

            return Built[ident] = typB.CreateType();
        }
    }
}
