using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Reflection.Emit;

namespace Bede.Thallium.Content
{
    using Belt;

    using Pair = Tuple<Func<ParameterInfo, bool>, Func<ParameterInfo, MethodInfo>>;

    /// <summary>
    /// A type that operates the <see cref="IContentBuilder" />
    /// </summary>
    public interface IImp
    {
        /// <summary>
        /// Process the body parameters as specified by the call description using
        /// the builder found atop the stack
        /// </summary>
        /// <param name="ilG"></param>
        /// <param name="call"></param>
        void Process(ILGenerator ilG, Description call);
    }

#pragma warning disable 1591

    public class Imp : List<Pair>, IImp
    {
        readonly static Type Cbi = typeof(IContentBuilder);

        static Func<ParameterInfo, MethodInfo> Some(MethodInfo info)
        {
            return x => info;
        }

        static Func<ParameterInfo, bool> Assignable(Type type)
        {
            return x => type.IsAssignableFrom(x.ParameterType);
        }

        static MethodInfo Object(ParameterInfo parameter)
        {
            var ms = Cbi.GetMethod("Struct");
            var mo = Cbi.GetMethod("Object");

            var pt = parameter.ParameterType;

            var et = pt.IsNullable() ? pt.GetGenericArguments()[0] : pt;

            return et.IsValueType ? ms.MakeGenericMethod(et) : mo.MakeGenericMethod(et);
        }

        Func<ParameterInfo, MethodInfo> _fallback;

        public Imp()
        {
            Defaults();
        }

        void Defaults()
        {
            Bind(typeof(string), Cbi.GetMethod("String"));
            Bind(typeof(Stream), Cbi.GetMethod("Stream"));
            Bind(typeof(byte[]), Cbi.GetMethod("Bytes"));

            Fallback(Object);
        }

        public void Fallback(Func<ParameterInfo, MethodInfo> fallback)
        {
            _fallback = fallback;
        }

        public void Bind(Type type, MethodInfo binding)
        {
            Add(new Pair(Assignable(type), Some(binding)));
        }

        public MethodInfo this[ParameterInfo parameter]
        {
            get
            {
                var bound = this.LastOrDefault(x => x.Item1(parameter));

                return null == bound ? _fallback(parameter) : bound.Item2(parameter);
            }
        }

        public virtual void Process(ILGenerator ilG, Description call)
        {
            ilG.EmitString(call.Subtype);
            ilG.EmitString(call.Boundary);
            ilG.Emit(OpCodes.Call, Multi);

            foreach (var kv in call.Body)
            {
                Process(ilG, kv.Value, kv.Key);
            }

            ilG.Emit(OpCodes.Call, Build);
        }

        protected virtual void Process(ILGenerator ilG, ContentDescription opts, ParameterInfo bodyI)
        {
            var bodyT = bodyI.ParameterType;

            ilG.Emit(OpCodes.Ldarg, bodyI.Position + 1);
            ilG.EmitNullConversion(bodyT);

            var mi = this[bodyI];

            ilG.Emit(OpCodes.Call, mi);

            PostProcess(ilG, opts, bodyI);
        }

        protected virtual void PostProcess(ILGenerator ilG, ContentDescription opts, ParameterInfo bodyI)
        {
            var bodyT = bodyI.ParameterType;

            var ct = opts.Type;
            var di = opts.Disposition;

            if (null != ct)
            {
                ilG.Emit(OpCodes.Ldstr, ct);
                ilG.Emit(OpCodes.Call, typeof(Rfc2616).GetMethod("ContentType", new [] { typeof(string) }));
                ilG.Emit(OpCodes.Call, ContentType);
            }

            if (null != di)
            {
                ilG.Emit(OpCodes.Ldstr, di);
                ilG.Emit(OpCodes.Call, typeof(Rfc2616).GetMethod("Disposition", new [] { typeof(string) }));
                ilG.Emit(OpCodes.Call, ContentDisposition);

                if (opts.SetName)
                {
                    ilG.Emit(OpCodes.Ldstr, bodyI.Name);
                    ilG.Emit(OpCodes.Call, typeof(ContentBuilderExtensions).GetMethod("Name"));
                }

                if (typeof(FileStream).IsAssignableFrom(bodyT) && opts.SetFileName)
                {
                    ilG.Emit(OpCodes.Ldarg, bodyI.Position + 1);
                    ilG.Emit(OpCodes.Call, typeof(ContentBuilderExtensions).GetMethod("File", new [] { Cbi, typeof(FileStream) }));
                }
            }
        }

        public MethodInfo Multi
        {
            get { return Cbi.GetMethod("Multi"); }
        }

        public MethodInfo ContentType
        {
            get { return Cbi.GetMethod("ContentType"); }
        }

        public MethodInfo ContentDisposition
        {
            get { return Cbi.GetMethod("ContentDisposition"); }
        }

        public MethodInfo Reduce
        {
            get { return Cbi.GetMethod("Reduce"); }
        }

        public MethodInfo Build
        {
            get { return typeof(IBuilder<HttpContent>).GetMethod("Build"); }
        }
    }
}
