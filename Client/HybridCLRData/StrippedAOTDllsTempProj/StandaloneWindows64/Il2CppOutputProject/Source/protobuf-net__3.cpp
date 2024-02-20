#include "pch-cpp.hpp"

#ifndef _MSC_VER
# include <alloca.h>
#else
# include <malloc.h>
#endif


#include <limits>


template <typename T1>
struct VirtualActionInvoker1
{
	typedef void (*Action)(void*, T1, const RuntimeMethod*);

	static inline void Invoke (Il2CppMethodSlot slot, RuntimeObject* obj, T1 p1)
	{
		const VirtualInvokeData& invokeData = il2cpp_codegen_get_virtual_invoke_data(slot, obj);
		((Action)invokeData.methodPtr)(obj, p1, invokeData.method);
	}
};
template <typename R>
struct VirtualFuncInvoker0
{
	typedef R (*Func)(void*, const RuntimeMethod*);

	static inline R Invoke (Il2CppMethodSlot slot, RuntimeObject* obj)
	{
		const VirtualInvokeData& invokeData = il2cpp_codegen_get_virtual_invoke_data(slot, obj);
		return ((Func)invokeData.methodPtr)(obj, invokeData.method);
	}
};
template <typename R, typename T1>
struct VirtualFuncInvoker1
{
	typedef R (*Func)(void*, T1, const RuntimeMethod*);

	static inline R Invoke (Il2CppMethodSlot slot, RuntimeObject* obj, T1 p1)
	{
		const VirtualInvokeData& invokeData = il2cpp_codegen_get_virtual_invoke_data(slot, obj);
		return ((Func)invokeData.methodPtr)(obj, p1, invokeData.method);
	}
};
template <typename T1>
struct GenericVirtualActionInvoker1
{
	typedef void (*Action)(void*, T1, const RuntimeMethod*);

	static inline void Invoke (const RuntimeMethod* method, RuntimeObject* obj, T1 p1)
	{
		VirtualInvokeData invokeData;
		il2cpp_codegen_get_generic_virtual_invoke_data(method, obj, &invokeData);
		((Action)invokeData.methodPtr)(obj, p1, invokeData.method);
	}
};
template <typename R, typename T1>
struct GenericVirtualFuncInvoker1
{
	typedef R (*Func)(void*, T1, const RuntimeMethod*);

	static inline R Invoke (const RuntimeMethod* method, RuntimeObject* obj, T1 p1)
	{
		VirtualInvokeData invokeData;
		il2cpp_codegen_get_generic_virtual_invoke_data(method, obj, &invokeData);
		return ((Func)invokeData.methodPtr)(obj, p1, invokeData.method);
	}
};
struct InterfaceActionInvoker0
{
	typedef void (*Action)(void*, const RuntimeMethod*);

	static inline void Invoke (Il2CppMethodSlot slot, RuntimeClass* declaringInterface, RuntimeObject* obj)
	{
		const VirtualInvokeData& invokeData = il2cpp_codegen_get_interface_invoke_data(slot, obj, declaringInterface);
		((Action)invokeData.methodPtr)(obj, invokeData.method);
	}
};
template <typename T1>
struct InterfaceActionInvoker1
{
	typedef void (*Action)(void*, T1, const RuntimeMethod*);

	static inline void Invoke (Il2CppMethodSlot slot, RuntimeClass* declaringInterface, RuntimeObject* obj, T1 p1)
	{
		const VirtualInvokeData& invokeData = il2cpp_codegen_get_interface_invoke_data(slot, obj, declaringInterface);
		((Action)invokeData.methodPtr)(obj, p1, invokeData.method);
	}
};
template <typename R, typename T1>
struct InterfaceFuncInvoker1
{
	typedef R (*Func)(void*, T1, const RuntimeMethod*);

	static inline R Invoke (Il2CppMethodSlot slot, RuntimeClass* declaringInterface, RuntimeObject* obj, T1 p1)
	{
		const VirtualInvokeData& invokeData = il2cpp_codegen_get_interface_invoke_data(slot, obj, declaringInterface);
		return ((Func)invokeData.methodPtr)(obj, p1, invokeData.method);
	}
};
template <typename T1>
struct GenericInterfaceActionInvoker1
{
	typedef void (*Action)(void*, T1, const RuntimeMethod*);

	static inline void Invoke (const RuntimeMethod* method, RuntimeObject* obj, T1 p1)
	{
		VirtualInvokeData invokeData;
		il2cpp_codegen_get_generic_interface_invoke_data(method, obj, &invokeData);
		((Action)invokeData.methodPtr)(obj, p1, invokeData.method);
	}
};
template <typename R, typename T1>
struct GenericInterfaceFuncInvoker1
{
	typedef R (*Func)(void*, T1, const RuntimeMethod*);

	static inline R Invoke (const RuntimeMethod* method, RuntimeObject* obj, T1 p1)
	{
		VirtualInvokeData invokeData;
		il2cpp_codegen_get_generic_interface_invoke_data(method, obj, &invokeData);
		return ((Func)invokeData.methodPtr)(obj, p1, invokeData.method);
	}
};

struct Dictionary_2_t46B2DB028096FA2B828359E52F37F3105A83AD83;
struct ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031;
struct CharU5BU5D_t799905CF001DD5F13F7DBB310181FC4D8B7D0AAB;
struct DelegateU5BU5D_tC5AB7E8F745616680F337909D3A8E6C722CDF771;
struct IntPtrU5BU5D_tFD177F8C806A6921AD7150264CCC62FA00CAD832;
struct StackTraceU5BU5D_t32FBCB20930EAF5BAE3F450FF75228E5450DA0DF;
struct TypeU5BU5D_t97234E1129B564EB38B8D85CAC2AD8B5B9522FFB;
struct SerializerPairU5BU5D_t0649E7685A7A4F4AEEDE3B16A66ADB0A843FCF4F;
struct ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129;
struct AsyncCallback_t7FEF460CBDCFB9C5FA2EF776984778B9A4145F4C;
struct BasicList_tC599C4B0AA650758C3D05E66CBA7402B9A774D69;
struct Binder_t91BFCE95A7057FADF4D8A1A342AFE52872246235;
struct CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8;
struct Delegate_t;
struct DelegateData_t9B286B493293CD2D23A5B2B5EF0E5B1324C2B77E;
struct DynamicMethod_t11548939953F8E4E3130471EBFACF0E9EBDCCA83;
struct IAsyncResult_t7B9B5A0ECB35DCEC31B8A8122C37D687369253B5;
struct IDictionary_t6D03155AF1FA9083817AA5B6AD7DEEACC26AB220;
struct ILGenerator_t2B624D1AA8840E0A7147E6628C5606D42443617A;
struct Local_tD3297DE40276C3DB9E44DABFB2DF7A8C4477557B;
struct LocalBuilder_tCB5AF342391604CED7394B81A931AB5BC6B97BB6;
struct MemberFilter_tF644F1AE82F611B677CE1964D5A3277DDA21D553;
struct MethodInfo_t;
struct MutableList_t3120C0EBF1975894C31018A3EFC96D9F11068EE7;
struct NetObjectCache_tDB28B2926C3CFE948B91DC26FB03E4702B77E8D7;
struct ObjectDisposedException_tC5FB29E8E980E2010A2F6A5B9B791089419F89EB;
struct ProtoDeserializer_t9C33607A0BAC6A3A9B5FFD51FE8B51EF52CA61E5;
struct ProtoReader_t894D20C36B7D7B81DC5345A22A8FF4E72F83AD53;
struct ProtoSerializer_tFEA7AC4576A3AF4078DF2F150739086926FFE43E;
struct ProtoWriter_tB8E54CFB9AA58B570DD20AB90ED11B461D4F90D8;
struct SafeSerializationManager_tCBB85B95DFD1634237140CD892E82D06ECB3F5E6;
struct SerializationContext_tD7B9F71DD1B20FDCE4BEFD102AB83AC7B6347C7B;
struct Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE;
struct String_t;
struct Type_t;
struct TypeModel_t79413F12948B54E302EC6251794665060FAF33F1;
struct UTF8Encoding_t90B56215A1B0B7ED5CDEA772E695F0DDAFBCD3BE;
struct Void_t4861ACF8F4594C3437BB48B6E56783494B843915;
struct UsingBlock_t83B5B7E5EB3B3C88653D9841965A57E31D935A64;

IL2CPP_EXTERN_C RuntimeClass* ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* Helpers_t5FF1D70D730A4372BEEB10C28E7158A4344894C0_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* IDisposable_t030E0496B4E0E4E4F086825007979AF51F7248C5_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* Local_tD3297DE40276C3DB9E44DABFB2DF7A8C4477557B_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* ObjectDisposedException_tC5FB29E8E980E2010A2F6A5B9B791089419F89EB_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* Type_t_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C String_t* _stringLiteral062880938476E2B20298E6FC08CB87B48E5C7853;
IL2CPP_EXTERN_C String_t* _stringLiteral0E113F97D77FF0E45934C610A144C2819DC64A04;
IL2CPP_EXTERN_C String_t* _stringLiteralDEA070F4AC99A78BF17CB1D8CC143F070CD0A3B7;
IL2CPP_EXTERN_C const RuntimeMethod* Local_get_Value_m22B518C4BE370270903E843E59051D77439DCD46_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* UsingBlock__ctor_mD3F4B37419992FF243FC773318A3BFF2FD099FED_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeType* IDisposable_t030E0496B4E0E4E4F086825007979AF51F7248C5_0_0_0_var;
struct Delegate_t_marshaled_com;
struct Delegate_t_marshaled_pinvoke;
struct Exception_t_marshaled_com;
struct Exception_t_marshaled_pinvoke;

struct DelegateU5BU5D_tC5AB7E8F745616680F337909D3A8E6C722CDF771;

IL2CPP_EXTERN_C_BEGIN
IL2CPP_EXTERN_C_END

#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
struct Local_tD3297DE40276C3DB9E44DABFB2DF7A8C4477557B  : public RuntimeObject
{
	LocalBuilder_tCB5AF342391604CED7394B81A931AB5BC6B97BB6* ___value;
	Type_t* ___type;
	CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8* ___ctx;
};
struct LocalVariableInfo_tFEE59A3613A442977805D456B016C3592CE60B62  : public RuntimeObject
{
	Type_t* ___type;
	bool ___is_pinned;
	uint16_t ___position;
};
struct LocalVariableInfo_tFEE59A3613A442977805D456B016C3592CE60B62_marshaled_pinvoke
{
	Type_t* ___type;
	int32_t ___is_pinned;
	uint16_t ___position;
};
struct LocalVariableInfo_tFEE59A3613A442977805D456B016C3592CE60B62_marshaled_com
{
	Type_t* ___type;
	int32_t ___is_pinned;
	uint16_t ___position;
};
struct MemberInfo_t  : public RuntimeObject
{
};
struct String_t  : public RuntimeObject
{
	int32_t ____stringLength;
	Il2CppChar ____firstChar;
};
struct ValueType_t6D9B272BD21782F0A9A14F2E41F85A50E97A986F  : public RuntimeObject
{
};
struct ValueType_t6D9B272BD21782F0A9A14F2E41F85A50E97A986F_marshaled_pinvoke
{
};
struct ValueType_t6D9B272BD21782F0A9A14F2E41F85A50E97A986F_marshaled_com
{
};
struct Boolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22 
{
	bool ___m_value;
};
struct Enum_t2A1A94B24E3B776EEF4E5E485E290BB9D4D072E2  : public ValueType_t6D9B272BD21782F0A9A14F2E41F85A50E97A986F
{
};
struct Enum_t2A1A94B24E3B776EEF4E5E485E290BB9D4D072E2_marshaled_pinvoke
{
};
struct Enum_t2A1A94B24E3B776EEF4E5E485E290BB9D4D072E2_marshaled_com
{
};
struct IntPtr_t 
{
	void* ___m_value;
};
struct Label_tE539F0FEBA09A9E2042C249C75886A37351A89DE 
{
	int32_t ___label;
};
struct LocalBuilder_tCB5AF342391604CED7394B81A931AB5BC6B97BB6  : public LocalVariableInfo_tFEE59A3613A442977805D456B016C3592CE60B62
{
	String_t* ___name;
	ILGenerator_t2B624D1AA8840E0A7147E6628C5606D42443617A* ___ilgen;
	int32_t ___startOffset;
	int32_t ___endOffset;
};
struct LocalBuilder_tCB5AF342391604CED7394B81A931AB5BC6B97BB6_marshaled_pinvoke : public LocalVariableInfo_tFEE59A3613A442977805D456B016C3592CE60B62_marshaled_pinvoke
{
	char* ___name;
	ILGenerator_t2B624D1AA8840E0A7147E6628C5606D42443617A* ___ilgen;
	int32_t ___startOffset;
	int32_t ___endOffset;
};
struct LocalBuilder_tCB5AF342391604CED7394B81A931AB5BC6B97BB6_marshaled_com : public LocalVariableInfo_tFEE59A3613A442977805D456B016C3592CE60B62_marshaled_com
{
	Il2CppChar* ___name;
	ILGenerator_t2B624D1AA8840E0A7147E6628C5606D42443617A* ___ilgen;
	int32_t ___startOffset;
	int32_t ___endOffset;
};
struct MethodBase_t  : public MemberInfo_t
{
};
struct Void_t4861ACF8F4594C3437BB48B6E56783494B843915 
{
	union
	{
		struct
		{
		};
		uint8_t Void_t4861ACF8F4594C3437BB48B6E56783494B843915__padding[1];
	};
};
struct CodeLabel_t414DB2EC7CFAFEFEB711DD82A3203D7627909D65 
{
	Label_tE539F0FEBA09A9E2042C249C75886A37351A89DE ___Value;
	int32_t ___Index;
};
struct Delegate_t  : public RuntimeObject
{
	intptr_t ___method_ptr;
	intptr_t ___invoke_impl;
	RuntimeObject* ___m_target;
	intptr_t ___method;
	intptr_t ___delegate_trampoline;
	intptr_t ___extra_arg;
	intptr_t ___method_code;
	intptr_t ___interp_method;
	intptr_t ___interp_invoke_impl;
	MethodInfo_t* ___method_info;
	MethodInfo_t* ___original_method_info;
	DelegateData_t9B286B493293CD2D23A5B2B5EF0E5B1324C2B77E* ___data;
	bool ___method_is_virtual;
};
struct Delegate_t_marshaled_pinvoke
{
	intptr_t ___method_ptr;
	intptr_t ___invoke_impl;
	Il2CppIUnknown* ___m_target;
	intptr_t ___method;
	intptr_t ___delegate_trampoline;
	intptr_t ___extra_arg;
	intptr_t ___method_code;
	intptr_t ___interp_method;
	intptr_t ___interp_invoke_impl;
	MethodInfo_t* ___method_info;
	MethodInfo_t* ___original_method_info;
	DelegateData_t9B286B493293CD2D23A5B2B5EF0E5B1324C2B77E* ___data;
	int32_t ___method_is_virtual;
};
struct Delegate_t_marshaled_com
{
	intptr_t ___method_ptr;
	intptr_t ___invoke_impl;
	Il2CppIUnknown* ___m_target;
	intptr_t ___method;
	intptr_t ___delegate_trampoline;
	intptr_t ___extra_arg;
	intptr_t ___method_code;
	intptr_t ___interp_method;
	intptr_t ___interp_invoke_impl;
	MethodInfo_t* ___method_info;
	MethodInfo_t* ___original_method_info;
	DelegateData_t9B286B493293CD2D23A5B2B5EF0E5B1324C2B77E* ___data;
	int32_t ___method_is_virtual;
};
struct Exception_t  : public RuntimeObject
{
	String_t* ____className;
	String_t* ____message;
	RuntimeObject* ____data;
	Exception_t* ____innerException;
	String_t* ____helpURL;
	RuntimeObject* ____stackTrace;
	String_t* ____stackTraceString;
	String_t* ____remoteStackTraceString;
	int32_t ____remoteStackIndex;
	RuntimeObject* ____dynamicMethods;
	int32_t ____HResult;
	String_t* ____source;
	SafeSerializationManager_tCBB85B95DFD1634237140CD892E82D06ECB3F5E6* ____safeSerializationManager;
	StackTraceU5BU5D_t32FBCB20930EAF5BAE3F450FF75228E5450DA0DF* ___captured_traces;
	IntPtrU5BU5D_tFD177F8C806A6921AD7150264CCC62FA00CAD832* ___native_trace_ips;
	int32_t ___caught_in_unmanaged;
};
struct Exception_t_marshaled_pinvoke
{
	char* ____className;
	char* ____message;
	RuntimeObject* ____data;
	Exception_t_marshaled_pinvoke* ____innerException;
	char* ____helpURL;
	Il2CppIUnknown* ____stackTrace;
	char* ____stackTraceString;
	char* ____remoteStackTraceString;
	int32_t ____remoteStackIndex;
	Il2CppIUnknown* ____dynamicMethods;
	int32_t ____HResult;
	char* ____source;
	SafeSerializationManager_tCBB85B95DFD1634237140CD892E82D06ECB3F5E6* ____safeSerializationManager;
	StackTraceU5BU5D_t32FBCB20930EAF5BAE3F450FF75228E5450DA0DF* ___captured_traces;
	Il2CppSafeArray* ___native_trace_ips;
	int32_t ___caught_in_unmanaged;
};
struct Exception_t_marshaled_com
{
	Il2CppChar* ____className;
	Il2CppChar* ____message;
	RuntimeObject* ____data;
	Exception_t_marshaled_com* ____innerException;
	Il2CppChar* ____helpURL;
	Il2CppIUnknown* ____stackTrace;
	Il2CppChar* ____stackTraceString;
	Il2CppChar* ____remoteStackTraceString;
	int32_t ____remoteStackIndex;
	Il2CppIUnknown* ____dynamicMethods;
	int32_t ____HResult;
	Il2CppChar* ____source;
	SafeSerializationManager_tCBB85B95DFD1634237140CD892E82D06ECB3F5E6* ____safeSerializationManager;
	StackTraceU5BU5D_t32FBCB20930EAF5BAE3F450FF75228E5450DA0DF* ___captured_traces;
	Il2CppSafeArray* ___native_trace_ips;
	int32_t ___caught_in_unmanaged;
};
struct MethodInfo_t  : public MethodBase_t
{
};
struct RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B 
{
	intptr_t ___value;
};
struct WireType_t2128062EF398F44F7671AA6510A85B130782E71B 
{
	int32_t ___value__;
};
struct ILVersion_t9CDB0D49244B0527F93D0401A3BA1F06426BEB36 
{
	int32_t ___value__;
};
struct CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8  : public RuntimeObject
{
	DynamicMethod_t11548939953F8E4E3130471EBFACF0E9EBDCCA83* ___method;
	bool ___isStatic;
	SerializerPairU5BU5D_t0649E7685A7A4F4AEEDE3B16A66ADB0A843FCF4F* ___methodPairs;
	bool ___isWriter;
	bool ___nonPublic;
	Local_tD3297DE40276C3DB9E44DABFB2DF7A8C4477557B* ___inputValue;
	String_t* ___assemblyName;
	ILGenerator_t2B624D1AA8840E0A7147E6628C5606D42443617A* ___il;
	MutableList_t3120C0EBF1975894C31018A3EFC96D9F11068EE7* ___locals;
	int32_t ___nextLabel;
	BasicList_tC599C4B0AA650758C3D05E66CBA7402B9A774D69* ___knownTrustedAssemblies;
	BasicList_tC599C4B0AA650758C3D05E66CBA7402B9A774D69* ___knownUntrustedAssemblies;
	TypeModel_t79413F12948B54E302EC6251794665060FAF33F1* ___model;
	int32_t ___metadataVersion;
};
struct MulticastDelegate_t  : public Delegate_t
{
	DelegateU5BU5D_tC5AB7E8F745616680F337909D3A8E6C722CDF771* ___delegates;
};
struct MulticastDelegate_t_marshaled_pinvoke : public Delegate_t_marshaled_pinvoke
{
	Delegate_t_marshaled_pinvoke** ___delegates;
};
struct MulticastDelegate_t_marshaled_com : public Delegate_t_marshaled_com
{
	Delegate_t_marshaled_com** ___delegates;
};
struct ProtoReader_t894D20C36B7D7B81DC5345A22A8FF4E72F83AD53  : public RuntimeObject
{
	Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* ___source;
	ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* ___ioBuffer;
	TypeModel_t79413F12948B54E302EC6251794665060FAF33F1* ___model;
	int32_t ___fieldNumber;
	int32_t ___depth;
	int32_t ___ioIndex;
	int32_t ___available;
	int64_t ___position64;
	int64_t ___blockEnd64;
	int64_t ___dataRemaining64;
	int32_t ___wireType;
	bool ___isFixedLength;
	bool ___internStrings;
	NetObjectCache_tDB28B2926C3CFE948B91DC26FB03E4702B77E8D7* ___netCache;
	uint32_t ___trapCount;
	SerializationContext_tD7B9F71DD1B20FDCE4BEFD102AB83AC7B6347C7B* ___context;
	Dictionary_2_t46B2DB028096FA2B828359E52F37F3105A83AD83* ___stringInterner;
};
struct ProtoWriter_tB8E54CFB9AA58B570DD20AB90ED11B461D4F90D8  : public RuntimeObject
{
	Stream_tF844051B786E8F7F4244DBD218D74E8617B9A2DE* ___dest;
	TypeModel_t79413F12948B54E302EC6251794665060FAF33F1* ___model;
	NetObjectCache_tDB28B2926C3CFE948B91DC26FB03E4702B77E8D7* ___netCache;
	int32_t ___fieldNumber;
	int32_t ___flushLock;
	int32_t ___wireType;
	int32_t ___depth;
	MutableList_t3120C0EBF1975894C31018A3EFC96D9F11068EE7* ___recursionStack;
	SerializationContext_tD7B9F71DD1B20FDCE4BEFD102AB83AC7B6347C7B* ___context;
	ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* ___ioBuffer;
	int32_t ___ioIndex;
	int64_t ___position64;
	int32_t ___packedFieldNumber;
};
struct SystemException_tCC48D868298F4C0705279823E34B00F4FBDB7295  : public Exception_t
{
};
struct Type_t  : public MemberInfo_t
{
	RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B ____impl;
};
struct UsingBlock_t83B5B7E5EB3B3C88653D9841965A57E31D935A64  : public RuntimeObject
{
	Local_tD3297DE40276C3DB9E44DABFB2DF7A8C4477557B* ___local;
	CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8* ___ctx;
	CodeLabel_t414DB2EC7CFAFEFEB711DD82A3203D7627909D65 ___label;
};
struct ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263  : public SystemException_tCC48D868298F4C0705279823E34B00F4FBDB7295
{
	String_t* ____paramName;
};
struct AsyncCallback_t7FEF460CBDCFB9C5FA2EF776984778B9A4145F4C  : public MulticastDelegate_t
{
};
struct InvalidOperationException_t5DDE4D49B7405FAAB1E4576F4715A42A3FAD4BAB  : public SystemException_tCC48D868298F4C0705279823E34B00F4FBDB7295
{
};
struct ProtoDeserializer_t9C33607A0BAC6A3A9B5FFD51FE8B51EF52CA61E5  : public MulticastDelegate_t
{
};
struct ProtoSerializer_tFEA7AC4576A3AF4078DF2F150739086926FFE43E  : public MulticastDelegate_t
{
};
struct ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129  : public ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263
{
};
struct ObjectDisposedException_tC5FB29E8E980E2010A2F6A5B9B791089419F89EB  : public InvalidOperationException_t5DDE4D49B7405FAAB1E4576F4715A42A3FAD4BAB
{
	String_t* ____objectName;
};
struct String_t_StaticFields
{
	String_t* ___Empty;
};
struct Boolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_StaticFields
{
	String_t* ___TrueString;
	String_t* ___FalseString;
};
struct IntPtr_t_StaticFields
{
	intptr_t ___Zero;
};
struct CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8_StaticFields
{
	int32_t ___next;
};
struct ProtoReader_t894D20C36B7D7B81DC5345A22A8FF4E72F83AD53_StaticFields
{
	UTF8Encoding_t90B56215A1B0B7ED5CDEA772E695F0DDAFBCD3BE* ___encoding;
	ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* ___EmptyBlob;
};
struct ProtoReader_t894D20C36B7D7B81DC5345A22A8FF4E72F83AD53_ThreadStaticFields
{
	ProtoReader_t894D20C36B7D7B81DC5345A22A8FF4E72F83AD53* ___lastReader;
};
struct ProtoWriter_tB8E54CFB9AA58B570DD20AB90ED11B461D4F90D8_StaticFields
{
	UTF8Encoding_t90B56215A1B0B7ED5CDEA772E695F0DDAFBCD3BE* ___encoding;
};
struct Type_t_StaticFields
{
	Binder_t91BFCE95A7057FADF4D8A1A342AFE52872246235* ___s_defaultBinder;
	Il2CppChar ___Delimiter;
	TypeU5BU5D_t97234E1129B564EB38B8D85CAC2AD8B5B9522FFB* ___EmptyTypes;
	RuntimeObject* ___Missing;
	MemberFilter_tF644F1AE82F611B677CE1964D5A3277DDA21D553* ___FilterAttribute;
	MemberFilter_tF644F1AE82F611B677CE1964D5A3277DDA21D553* ___FilterName;
	MemberFilter_tF644F1AE82F611B677CE1964D5A3277DDA21D553* ___FilterNameIgnoreCase;
};
#ifdef __clang__
#pragma clang diagnostic pop
#endif
struct DelegateU5BU5D_tC5AB7E8F745616680F337909D3A8E6C722CDF771  : public RuntimeArray
{
	ALIGN_FIELD (8) Delegate_t* m_Items[1];

	inline Delegate_t* GetAt(il2cpp_array_size_t index) const
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items[index];
	}
	inline Delegate_t** GetAddressAt(il2cpp_array_size_t index)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items + index;
	}
	inline void SetAt(il2cpp_array_size_t index, Delegate_t* value)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)m_Items + index, (void*)value);
	}
	inline Delegate_t* GetAtUnchecked(il2cpp_array_size_t index) const
	{
		return m_Items[index];
	}
	inline Delegate_t** GetAddressAtUnchecked(il2cpp_array_size_t index)
	{
		return m_Items + index;
	}
	inline void SetAtUnchecked(il2cpp_array_size_t index, Delegate_t* value)
	{
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)m_Items + index, (void*)value);
	}
};



IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2 (RuntimeObject* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ArgumentNullException__ctor_m444AE141157E333844FC1A9500224C2F9FD24F4B (ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129* __this, String_t* ___0_paramName, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Type_t* Local_get_Type_m770B7E893A9310B76D58CAB864EF51701C05FB94_inline (Local_tD3297DE40276C3DB9E44DABFB2DF7A8C4477557B* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Helpers_IsValueType_m342FDB2C0B40EC16DC8A6FF32974376D0DC1640B (Type_t* ___0_type, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Helpers_IsSealed_m2F25A27E41E7E699310681F6DD6EB9511E7354C9 (Type_t* ___0_type, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Type_t* Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57 (RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B ___0_handle, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Type_t* CompilerContext_MapType_m5A552FC738BC790DF41D4A2BFD87FF4351231031 (CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8* __this, Type_t* ___0_type, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR CodeLabel_t414DB2EC7CFAFEFEB711DD82A3203D7627909D65 CompilerContext_BeginTry_m538A277EC3B8665B72580A6D78D64384F0524514 (CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void CompilerContext_EndTry_m13A4D9DD12A0A5ED5E3C3DF054AE0FBFD242358B (CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8* __this, CodeLabel_t414DB2EC7CFAFEFEB711DD82A3203D7627909D65 ___0_label, bool ___1_short, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void CompilerContext_BeginFinally_m7F0F8D090F69A77A097EEFE968740E547BB90C65 (CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR MethodInfo_t* Type_GetMethod_m66AD062187F19497DBCA900823B0C268322DC231 (Type_t* __this, String_t* ___0_name, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void CompilerContext_LoadAddress_m6E0D8DEFD1EBB7AEAEA1A1C11EA873B6F331564F (CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8* __this, Local_tD3297DE40276C3DB9E44DABFB2DF7A8C4477557B* ___0_local, Type_t* ___1_type, bool ___2_evenIfClass, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t CompilerContext_get_MetadataVersion_mE8212D3B0AE87B6D46E31B2A42FE196C2D7A1DAE_inline (CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void CompilerContext_LoadValue_m4D5BA3FC6E43E3F7D5D6631975B51787A1ED24EF (CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8* __this, Local_tD3297DE40276C3DB9E44DABFB2DF7A8C4477557B* ___0_local, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void CompilerContext_CastToObject_mBECE35F44684B56DA2DC9F8C2AEC7AE1B5477B5B (CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8* __this, Type_t* ___0_type, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void CompilerContext_Constrain_m90FF59FB0A6856EFC4604DE590E26C69F74DC2E8 (CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8* __this, Type_t* ___0_type, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void CompilerContext_EmitCall_m8A75926A8A4CB3DF28015C7752834966AB7E6DFA (CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8* __this, MethodInfo_t* ___0_method, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR CodeLabel_t414DB2EC7CFAFEFEB711DD82A3203D7627909D65 CompilerContext_DefineLabel_m6A9CB8B0A40373AED997CD398B0B02896D010812 (CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void CompilerContext_BranchIfFalse_m28FA64179D958432C1C16F5834627C0D404BB74C (CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8* __this, CodeLabel_t414DB2EC7CFAFEFEB711DD82A3203D7627909D65 ___0_label, bool ___1_short, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Local__ctor_mA3FAAB90E8C52DFEA913667FACB9E44DC3AE5E19 (Local_tD3297DE40276C3DB9E44DABFB2DF7A8C4477557B* __this, CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8* ___0_ctx, Type_t* ___1_type, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void CompilerContext_TryCast_mAA046B66AFDFE27F723F4D6B20F1DDFA80D59C1B (CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8* __this, Type_t* ___0_type, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void CompilerContext_CopyValue_m4E160182C35472C290CA69F9E51C58175F02352B (CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void CompilerContext_StoreValue_mE2E10616FA6782B7803496CF575F7A2CBB49A125 (CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8* __this, Local_tD3297DE40276C3DB9E44DABFB2DF7A8C4477557B* ___0_local, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void CompilerContext_MarkLabel_m4AEF6BD198470616CD9DAFF60C2CFAF31F7103CC (CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8* __this, CodeLabel_t414DB2EC7CFAFEFEB711DD82A3203D7627909D65 ___0_label, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void CompilerContext_EndFinally_m284B5341D164C99BF210448333BFAF9A924EB4E6 (CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR LocalBuilder_tCB5AF342391604CED7394B81A931AB5BC6B97BB6* CompilerContext_GetFromPool_mF0FEA5084EACBC292A9FBE5E4A6D1229A34756F2 (CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8* __this, Type_t* ___0_type, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Type_t* Object_GetType_mE10A8FC1E57F3DF29972CCBC026C2DC3942263B3 (RuntimeObject* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ObjectDisposedException__ctor_mB2C8582279AF3F0C1CF9AA52DA7331BF848DFD48 (ObjectDisposedException_tC5FB29E8E980E2010A2F6A5B9B791089419F89EB* __this, String_t* ___0_objectName, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Local__ctor_mF48224DF5E9358C792AC309D0E9EF1C94EEC6525 (Local_tD3297DE40276C3DB9E44DABFB2DF7A8C4477557B* __this, LocalBuilder_tCB5AF342391604CED7394B81A931AB5BC6B97BB6* ___0_value, Type_t* ___1_type, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void CompilerContext_ReleaseToPool_m5B36FA843A95DE38B8D7A1627D11CE98F48B1327 (CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8* __this, LocalBuilder_tCB5AF342391604CED7394B81A931AB5BC6B97BB6* ___0_value, const RuntimeMethod* method) ;
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UsingBlock__ctor_mD3F4B37419992FF243FC773318A3BFF2FD099FED (UsingBlock_t83B5B7E5EB3B3C88653D9841965A57E31D935A64* __this, CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8* ___0_ctx, Local_tD3297DE40276C3DB9E44DABFB2DF7A8C4477557B* ___1_local, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Helpers_t5FF1D70D730A4372BEEB10C28E7158A4344894C0_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&IDisposable_t030E0496B4E0E4E4F086825007979AF51F7248C5_0_0_0_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Type_t_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	Type_t* V_0 = NULL;
	{
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2(__this, NULL);
		CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8* L_0 = ___0_ctx;
		if (L_0)
		{
			goto IL_0014;
		}
	}
	{
		ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129* L_1 = (ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129_il2cpp_TypeInfo_var)));
		ArgumentNullException__ctor_m444AE141157E333844FC1A9500224C2F9FD24F4B(L_1, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral062880938476E2B20298E6FC08CB87B48E5C7853)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_1, ((RuntimeMethod*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&UsingBlock__ctor_mD3F4B37419992FF243FC773318A3BFF2FD099FED_RuntimeMethod_var)));
	}

IL_0014:
	{
		Local_tD3297DE40276C3DB9E44DABFB2DF7A8C4477557B* L_2 = ___1_local;
		if (L_2)
		{
			goto IL_0022;
		}
	}
	{
		ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129* L_3 = (ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129_il2cpp_TypeInfo_var)));
		ArgumentNullException__ctor_m444AE141157E333844FC1A9500224C2F9FD24F4B(L_3, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralDEA070F4AC99A78BF17CB1D8CC143F070CD0A3B7)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_3, ((RuntimeMethod*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&UsingBlock__ctor_mD3F4B37419992FF243FC773318A3BFF2FD099FED_RuntimeMethod_var)));
	}

IL_0022:
	{
		Local_tD3297DE40276C3DB9E44DABFB2DF7A8C4477557B* L_4 = ___1_local;
		NullCheck(L_4);
		Type_t* L_5;
		L_5 = Local_get_Type_m770B7E893A9310B76D58CAB864EF51701C05FB94_inline(L_4, NULL);
		V_0 = L_5;
		Type_t* L_6 = V_0;
		il2cpp_codegen_runtime_class_init_inline(Helpers_t5FF1D70D730A4372BEEB10C28E7158A4344894C0_il2cpp_TypeInfo_var);
		bool L_7;
		L_7 = Helpers_IsValueType_m342FDB2C0B40EC16DC8A6FF32974376D0DC1640B(L_6, NULL);
		if (L_7)
		{
			goto IL_0039;
		}
	}
	{
		Type_t* L_8 = V_0;
		il2cpp_codegen_runtime_class_init_inline(Helpers_t5FF1D70D730A4372BEEB10C28E7158A4344894C0_il2cpp_TypeInfo_var);
		bool L_9;
		L_9 = Helpers_IsSealed_m2F25A27E41E7E699310681F6DD6EB9511E7354C9(L_8, NULL);
		if (!L_9)
		{
			goto IL_0052;
		}
	}

IL_0039:
	{
		CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8* L_10 = ___0_ctx;
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_11 = { reinterpret_cast<intptr_t> (IDisposable_t030E0496B4E0E4E4F086825007979AF51F7248C5_0_0_0_var) };
		il2cpp_codegen_runtime_class_init_inline(Type_t_il2cpp_TypeInfo_var);
		Type_t* L_12;
		L_12 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_11, NULL);
		NullCheck(L_10);
		Type_t* L_13;
		L_13 = CompilerContext_MapType_m5A552FC738BC790DF41D4A2BFD87FF4351231031(L_10, L_12, NULL);
		Type_t* L_14 = V_0;
		NullCheck(L_13);
		bool L_15;
		L_15 = VirtualFuncInvoker1< bool, Type_t* >::Invoke(167, L_13, L_14);
		if (L_15)
		{
			goto IL_0052;
		}
	}
	{
		return;
	}

IL_0052:
	{
		Local_tD3297DE40276C3DB9E44DABFB2DF7A8C4477557B* L_16 = ___1_local;
		__this->___local = L_16;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___local), (void*)L_16);
		CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8* L_17 = ___0_ctx;
		__this->___ctx = L_17;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___ctx), (void*)L_17);
		CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8* L_18 = ___0_ctx;
		NullCheck(L_18);
		CodeLabel_t414DB2EC7CFAFEFEB711DD82A3203D7627909D65 L_19;
		L_19 = CompilerContext_BeginTry_m538A277EC3B8665B72580A6D78D64384F0524514(L_18, NULL);
		__this->___label = L_19;
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void UsingBlock_Dispose_m89860A9BF721C125B14EF14341AF668217D09D55 (UsingBlock_t83B5B7E5EB3B3C88653D9841965A57E31D935A64* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Helpers_t5FF1D70D730A4372BEEB10C28E7158A4344894C0_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&IDisposable_t030E0496B4E0E4E4F086825007979AF51F7248C5_0_0_0_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&IDisposable_t030E0496B4E0E4E4F086825007979AF51F7248C5_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Local_tD3297DE40276C3DB9E44DABFB2DF7A8C4477557B_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Type_t_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral0E113F97D77FF0E45934C610A144C2819DC64A04);
		s_Il2CppMethodInitialized = true;
	}
	Type_t* V_0 = NULL;
	MethodInfo_t* V_1 = NULL;
	Type_t* V_2 = NULL;
	int32_t V_3 = 0;
	CodeLabel_t414DB2EC7CFAFEFEB711DD82A3203D7627909D65 V_4;
	memset((&V_4), 0, sizeof(V_4));
	Local_tD3297DE40276C3DB9E44DABFB2DF7A8C4477557B* V_5 = NULL;
	{
		Local_tD3297DE40276C3DB9E44DABFB2DF7A8C4477557B* L_0 = __this->___local;
		if (!L_0)
		{
			goto IL_0010;
		}
	}
	{
		CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8* L_1 = __this->___ctx;
		if (L_1)
		{
			goto IL_0011;
		}
	}

IL_0010:
	{
		return;
	}

IL_0011:
	{
		CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8* L_2 = __this->___ctx;
		CodeLabel_t414DB2EC7CFAFEFEB711DD82A3203D7627909D65 L_3 = __this->___label;
		NullCheck(L_2);
		CompilerContext_EndTry_m13A4D9DD12A0A5ED5E3C3DF054AE0FBFD242358B(L_2, L_3, (bool)0, NULL);
		CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8* L_4 = __this->___ctx;
		NullCheck(L_4);
		CompilerContext_BeginFinally_m7F0F8D090F69A77A097EEFE968740E547BB90C65(L_4, NULL);
		CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8* L_5 = __this->___ctx;
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_6 = { reinterpret_cast<intptr_t> (IDisposable_t030E0496B4E0E4E4F086825007979AF51F7248C5_0_0_0_var) };
		il2cpp_codegen_runtime_class_init_inline(Type_t_il2cpp_TypeInfo_var);
		Type_t* L_7;
		L_7 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_6, NULL);
		NullCheck(L_5);
		Type_t* L_8;
		L_8 = CompilerContext_MapType_m5A552FC738BC790DF41D4A2BFD87FF4351231031(L_5, L_7, NULL);
		V_0 = L_8;
		Type_t* L_9 = V_0;
		NullCheck(L_9);
		MethodInfo_t* L_10;
		L_10 = Type_GetMethod_m66AD062187F19497DBCA900823B0C268322DC231(L_9, _stringLiteral0E113F97D77FF0E45934C610A144C2819DC64A04, NULL);
		V_1 = L_10;
		Local_tD3297DE40276C3DB9E44DABFB2DF7A8C4477557B* L_11 = __this->___local;
		NullCheck(L_11);
		Type_t* L_12;
		L_12 = Local_get_Type_m770B7E893A9310B76D58CAB864EF51701C05FB94_inline(L_11, NULL);
		V_2 = L_12;
		Type_t* L_13 = V_2;
		il2cpp_codegen_runtime_class_init_inline(Helpers_t5FF1D70D730A4372BEEB10C28E7158A4344894C0_il2cpp_TypeInfo_var);
		bool L_14;
		L_14 = Helpers_IsValueType_m342FDB2C0B40EC16DC8A6FF32974376D0DC1640B(L_13, NULL);
		if (!L_14)
		{
			goto IL_00c2;
		}
	}
	{
		CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8* L_15 = __this->___ctx;
		Local_tD3297DE40276C3DB9E44DABFB2DF7A8C4477557B* L_16 = __this->___local;
		Type_t* L_17 = V_2;
		NullCheck(L_15);
		CompilerContext_LoadAddress_m6E0D8DEFD1EBB7AEAEA1A1C11EA873B6F331564F(L_15, L_16, L_17, (bool)0, NULL);
		CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8* L_18 = __this->___ctx;
		NullCheck(L_18);
		int32_t L_19;
		L_19 = CompilerContext_get_MetadataVersion_mE8212D3B0AE87B6D46E31B2A42FE196C2D7A1DAE_inline(L_18, NULL);
		V_3 = L_19;
		int32_t L_20 = V_3;
		if (L_20)
		{
			goto IL_00a5;
		}
	}
	{
		CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8* L_21 = __this->___ctx;
		Local_tD3297DE40276C3DB9E44DABFB2DF7A8C4477557B* L_22 = __this->___local;
		NullCheck(L_21);
		CompilerContext_LoadValue_m4D5BA3FC6E43E3F7D5D6631975B51787A1ED24EF(L_21, L_22, NULL);
		CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8* L_23 = __this->___ctx;
		Type_t* L_24 = V_2;
		NullCheck(L_23);
		CompilerContext_CastToObject_mBECE35F44684B56DA2DC9F8C2AEC7AE1B5477B5B(L_23, L_24, NULL);
		goto IL_00b1;
	}

IL_00a5:
	{
		CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8* L_25 = __this->___ctx;
		Type_t* L_26 = V_2;
		NullCheck(L_25);
		CompilerContext_Constrain_m90FF59FB0A6856EFC4604DE590E26C69F74DC2E8(L_25, L_26, NULL);
	}

IL_00b1:
	{
		CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8* L_27 = __this->___ctx;
		MethodInfo_t* L_28 = V_1;
		NullCheck(L_27);
		CompilerContext_EmitCall_m8A75926A8A4CB3DF28015C7752834966AB7E6DFA(L_27, L_28, NULL);
		goto IL_0193;
	}

IL_00c2:
	{
		CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8* L_29 = __this->___ctx;
		NullCheck(L_29);
		CodeLabel_t414DB2EC7CFAFEFEB711DD82A3203D7627909D65 L_30;
		L_30 = CompilerContext_DefineLabel_m6A9CB8B0A40373AED997CD398B0B02896D010812(L_29, NULL);
		V_4 = L_30;
		Type_t* L_31 = V_0;
		Type_t* L_32 = V_2;
		NullCheck(L_31);
		bool L_33;
		L_33 = VirtualFuncInvoker1< bool, Type_t* >::Invoke(167, L_31, L_32);
		if (!L_33)
		{
			goto IL_010c;
		}
	}
	{
		CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8* L_34 = __this->___ctx;
		Local_tD3297DE40276C3DB9E44DABFB2DF7A8C4477557B* L_35 = __this->___local;
		NullCheck(L_34);
		CompilerContext_LoadValue_m4D5BA3FC6E43E3F7D5D6631975B51787A1ED24EF(L_34, L_35, NULL);
		CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8* L_36 = __this->___ctx;
		CodeLabel_t414DB2EC7CFAFEFEB711DD82A3203D7627909D65 L_37 = V_4;
		NullCheck(L_36);
		CompilerContext_BranchIfFalse_m28FA64179D958432C1C16F5834627C0D404BB74C(L_36, L_37, (bool)1, NULL);
		CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8* L_38 = __this->___ctx;
		Local_tD3297DE40276C3DB9E44DABFB2DF7A8C4477557B* L_39 = __this->___local;
		Type_t* L_40 = V_2;
		NullCheck(L_38);
		CompilerContext_LoadAddress_m6E0D8DEFD1EBB7AEAEA1A1C11EA873B6F331564F(L_38, L_39, L_40, (bool)0, NULL);
		goto IL_017a;
	}

IL_010c:
	{
		CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8* L_41 = __this->___ctx;
		Type_t* L_42 = V_0;
		Local_tD3297DE40276C3DB9E44DABFB2DF7A8C4477557B* L_43 = (Local_tD3297DE40276C3DB9E44DABFB2DF7A8C4477557B*)il2cpp_codegen_object_new(Local_tD3297DE40276C3DB9E44DABFB2DF7A8C4477557B_il2cpp_TypeInfo_var);
		Local__ctor_mA3FAAB90E8C52DFEA913667FACB9E44DC3AE5E19(L_43, L_41, L_42, NULL);
		V_5 = L_43;
	}
	{
		auto __finallyBlock = il2cpp::utils::Finally([&]
		{

FINALLY_016e:
			{
				{
					Local_tD3297DE40276C3DB9E44DABFB2DF7A8C4477557B* L_44 = V_5;
					if (!L_44)
					{
						goto IL_0179;
					}
				}
				{
					Local_tD3297DE40276C3DB9E44DABFB2DF7A8C4477557B* L_45 = V_5;
					NullCheck(L_45);
					InterfaceActionInvoker0::Invoke(0, IDisposable_t030E0496B4E0E4E4F086825007979AF51F7248C5_il2cpp_TypeInfo_var, L_45);
				}

IL_0179:
				{
					return;
				}
			}
		});
		try
		{
			CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8* L_46 = __this->___ctx;
			Local_tD3297DE40276C3DB9E44DABFB2DF7A8C4477557B* L_47 = __this->___local;
			NullCheck(L_46);
			CompilerContext_LoadValue_m4D5BA3FC6E43E3F7D5D6631975B51787A1ED24EF(L_46, L_47, NULL);
			CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8* L_48 = __this->___ctx;
			Type_t* L_49 = V_0;
			NullCheck(L_48);
			CompilerContext_TryCast_mAA046B66AFDFE27F723F4D6B20F1DDFA80D59C1B(L_48, L_49, NULL);
			CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8* L_50 = __this->___ctx;
			NullCheck(L_50);
			CompilerContext_CopyValue_m4E160182C35472C290CA69F9E51C58175F02352B(L_50, NULL);
			CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8* L_51 = __this->___ctx;
			Local_tD3297DE40276C3DB9E44DABFB2DF7A8C4477557B* L_52 = V_5;
			NullCheck(L_51);
			CompilerContext_StoreValue_mE2E10616FA6782B7803496CF575F7A2CBB49A125(L_51, L_52, NULL);
			CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8* L_53 = __this->___ctx;
			CodeLabel_t414DB2EC7CFAFEFEB711DD82A3203D7627909D65 L_54 = V_4;
			NullCheck(L_53);
			CompilerContext_BranchIfFalse_m28FA64179D958432C1C16F5834627C0D404BB74C(L_53, L_54, (bool)1, NULL);
			CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8* L_55 = __this->___ctx;
			Local_tD3297DE40276C3DB9E44DABFB2DF7A8C4477557B* L_56 = V_5;
			Type_t* L_57 = V_0;
			NullCheck(L_55);
			CompilerContext_LoadAddress_m6E0D8DEFD1EBB7AEAEA1A1C11EA873B6F331564F(L_55, L_56, L_57, (bool)0, NULL);
			goto IL_017a;
		}
		catch(Il2CppExceptionWrapper& e)
		{
			__finallyBlock.StoreException(e.ex);
		}
	}

IL_017a:
	{
		CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8* L_58 = __this->___ctx;
		MethodInfo_t* L_59 = V_1;
		NullCheck(L_58);
		CompilerContext_EmitCall_m8A75926A8A4CB3DF28015C7752834966AB7E6DFA(L_58, L_59, NULL);
		CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8* L_60 = __this->___ctx;
		CodeLabel_t414DB2EC7CFAFEFEB711DD82A3203D7627909D65 L_61 = V_4;
		NullCheck(L_60);
		CompilerContext_MarkLabel_m4AEF6BD198470616CD9DAFF60C2CFAF31F7103CC(L_60, L_61, NULL);
	}

IL_0193:
	{
		CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8* L_62 = __this->___ctx;
		NullCheck(L_62);
		CompilerContext_EndFinally_m284B5341D164C99BF210448333BFAF9A924EB4E6(L_62, NULL);
		__this->___local = (Local_tD3297DE40276C3DB9E44DABFB2DF7A8C4477557B*)NULL;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___local), (void*)(Local_tD3297DE40276C3DB9E44DABFB2DF7A8C4477557B*)NULL);
		__this->___ctx = (CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8*)NULL;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___ctx), (void*)(CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8*)NULL);
		CodeLabel_t414DB2EC7CFAFEFEB711DD82A3203D7627909D65* L_63 = (CodeLabel_t414DB2EC7CFAFEFEB711DD82A3203D7627909D65*)(&__this->___label);
		il2cpp_codegen_initobj(L_63, sizeof(CodeLabel_t414DB2EC7CFAFEFEB711DD82A3203D7627909D65));
		return;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
void ProtoSerializer_Invoke_m98C4E7AD1411C69749B387F2EB68B10F2822B97A_Multicast(ProtoSerializer_tFEA7AC4576A3AF4078DF2F150739086926FFE43E* __this, RuntimeObject* ___0_value, ProtoWriter_tB8E54CFB9AA58B570DD20AB90ED11B461D4F90D8* ___1_dest, const RuntimeMethod* method)
{
	il2cpp_array_size_t length = __this->___delegates->max_length;
	Delegate_t** delegatesToInvoke = reinterpret_cast<Delegate_t**>(__this->___delegates->GetAddressAtUnchecked(0));
	for (il2cpp_array_size_t i = 0; i < length; i++)
	{
		ProtoSerializer_tFEA7AC4576A3AF4078DF2F150739086926FFE43E* currentDelegate = reinterpret_cast<ProtoSerializer_tFEA7AC4576A3AF4078DF2F150739086926FFE43E*>(delegatesToInvoke[i]);
		typedef void (*FunctionPointerType) (RuntimeObject*, RuntimeObject*, ProtoWriter_tB8E54CFB9AA58B570DD20AB90ED11B461D4F90D8*, const RuntimeMethod*);
		((FunctionPointerType)currentDelegate->___invoke_impl)((Il2CppObject*)currentDelegate->___method_code, ___0_value, ___1_dest, reinterpret_cast<RuntimeMethod*>(currentDelegate->___method));
	}
}
void ProtoSerializer_Invoke_m98C4E7AD1411C69749B387F2EB68B10F2822B97A_OpenInst(ProtoSerializer_tFEA7AC4576A3AF4078DF2F150739086926FFE43E* __this, RuntimeObject* ___0_value, ProtoWriter_tB8E54CFB9AA58B570DD20AB90ED11B461D4F90D8* ___1_dest, const RuntimeMethod* method)
{
	NullCheck(___0_value);
	typedef void (*FunctionPointerType) (RuntimeObject*, ProtoWriter_tB8E54CFB9AA58B570DD20AB90ED11B461D4F90D8*, const RuntimeMethod*);
	((FunctionPointerType)__this->___method_ptr)(___0_value, ___1_dest, method);
}
void ProtoSerializer_Invoke_m98C4E7AD1411C69749B387F2EB68B10F2822B97A_OpenStatic(ProtoSerializer_tFEA7AC4576A3AF4078DF2F150739086926FFE43E* __this, RuntimeObject* ___0_value, ProtoWriter_tB8E54CFB9AA58B570DD20AB90ED11B461D4F90D8* ___1_dest, const RuntimeMethod* method)
{
	typedef void (*FunctionPointerType) (RuntimeObject*, ProtoWriter_tB8E54CFB9AA58B570DD20AB90ED11B461D4F90D8*, const RuntimeMethod*);
	((FunctionPointerType)__this->___method_ptr)(___0_value, ___1_dest, method);
}
void ProtoSerializer_Invoke_m98C4E7AD1411C69749B387F2EB68B10F2822B97A_OpenVirtual(ProtoSerializer_tFEA7AC4576A3AF4078DF2F150739086926FFE43E* __this, RuntimeObject* ___0_value, ProtoWriter_tB8E54CFB9AA58B570DD20AB90ED11B461D4F90D8* ___1_dest, const RuntimeMethod* method)
{
	NullCheck(___0_value);
	VirtualActionInvoker1< ProtoWriter_tB8E54CFB9AA58B570DD20AB90ED11B461D4F90D8* >::Invoke(il2cpp_codegen_method_get_slot(method), ___0_value, ___1_dest);
}
void ProtoSerializer_Invoke_m98C4E7AD1411C69749B387F2EB68B10F2822B97A_OpenInterface(ProtoSerializer_tFEA7AC4576A3AF4078DF2F150739086926FFE43E* __this, RuntimeObject* ___0_value, ProtoWriter_tB8E54CFB9AA58B570DD20AB90ED11B461D4F90D8* ___1_dest, const RuntimeMethod* method)
{
	NullCheck(___0_value);
	InterfaceActionInvoker1< ProtoWriter_tB8E54CFB9AA58B570DD20AB90ED11B461D4F90D8* >::Invoke(il2cpp_codegen_method_get_slot(method), il2cpp_codegen_method_get_declaring_type(method), ___0_value, ___1_dest);
}
void ProtoSerializer_Invoke_m98C4E7AD1411C69749B387F2EB68B10F2822B97A_OpenGenericVirtual(ProtoSerializer_tFEA7AC4576A3AF4078DF2F150739086926FFE43E* __this, RuntimeObject* ___0_value, ProtoWriter_tB8E54CFB9AA58B570DD20AB90ED11B461D4F90D8* ___1_dest, const RuntimeMethod* method)
{
	NullCheck(___0_value);
	GenericVirtualActionInvoker1< ProtoWriter_tB8E54CFB9AA58B570DD20AB90ED11B461D4F90D8* >::Invoke(method, ___0_value, ___1_dest);
}
void ProtoSerializer_Invoke_m98C4E7AD1411C69749B387F2EB68B10F2822B97A_OpenGenericInterface(ProtoSerializer_tFEA7AC4576A3AF4078DF2F150739086926FFE43E* __this, RuntimeObject* ___0_value, ProtoWriter_tB8E54CFB9AA58B570DD20AB90ED11B461D4F90D8* ___1_dest, const RuntimeMethod* method)
{
	NullCheck(___0_value);
	GenericInterfaceActionInvoker1< ProtoWriter_tB8E54CFB9AA58B570DD20AB90ED11B461D4F90D8* >::Invoke(method, ___0_value, ___1_dest);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ProtoSerializer__ctor_m6823C016A853003D44A9F25E9EEECE722A5EB821 (ProtoSerializer_tFEA7AC4576A3AF4078DF2F150739086926FFE43E* __this, RuntimeObject* ___0_object, intptr_t ___1_method, const RuntimeMethod* method) 
{
	__this->___method_ptr = (intptr_t)il2cpp_codegen_get_method_pointer((RuntimeMethod*)___1_method);
	__this->___method = ___1_method;
	__this->___m_target = ___0_object;
	Il2CppCodeGenWriteBarrier((void**)(&__this->___m_target), (void*)___0_object);
	int parameterCount = il2cpp_codegen_method_parameter_count((RuntimeMethod*)___1_method);
	__this->___method_code = (intptr_t)__this;
	if (MethodIsStatic((RuntimeMethod*)___1_method))
	{
		bool isOpen = parameterCount == 2;
		if (isOpen)
			__this->___invoke_impl = (intptr_t)&ProtoSerializer_Invoke_m98C4E7AD1411C69749B387F2EB68B10F2822B97A_OpenStatic;
		else
			{
				__this->___invoke_impl = __this->___method_ptr;
				__this->___method_code = (intptr_t)__this->___m_target;
			}
	}
	else
	{
		bool isOpen = parameterCount == 1;
		if (isOpen)
		{
			if (__this->___method_is_virtual)
			{
				if (il2cpp_codegen_method_is_generic_instance_method((RuntimeMethod*)___1_method))
					if (il2cpp_codegen_method_is_interface_method((RuntimeMethod*)___1_method))
						__this->___invoke_impl = (intptr_t)&ProtoSerializer_Invoke_m98C4E7AD1411C69749B387F2EB68B10F2822B97A_OpenGenericInterface;
					else
						__this->___invoke_impl = (intptr_t)&ProtoSerializer_Invoke_m98C4E7AD1411C69749B387F2EB68B10F2822B97A_OpenGenericVirtual;
				else
					if (il2cpp_codegen_method_is_interface_method((RuntimeMethod*)___1_method))
						__this->___invoke_impl = (intptr_t)&ProtoSerializer_Invoke_m98C4E7AD1411C69749B387F2EB68B10F2822B97A_OpenInterface;
					else
						__this->___invoke_impl = (intptr_t)&ProtoSerializer_Invoke_m98C4E7AD1411C69749B387F2EB68B10F2822B97A_OpenVirtual;
			}
			else
			{
				__this->___invoke_impl = (intptr_t)&ProtoSerializer_Invoke_m98C4E7AD1411C69749B387F2EB68B10F2822B97A_OpenInst;
			}
		}
		else
		{
			if (___0_object == NULL)
				il2cpp_codegen_raise_exception(il2cpp_codegen_get_argument_exception(NULL, "Delegate to an instance method cannot have null 'this'."), NULL);
			__this->___invoke_impl = __this->___method_ptr;
			__this->___method_code = (intptr_t)__this->___m_target;
		}
	}
	__this->___extra_arg = (intptr_t)&ProtoSerializer_Invoke_m98C4E7AD1411C69749B387F2EB68B10F2822B97A_Multicast;
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ProtoSerializer_Invoke_m98C4E7AD1411C69749B387F2EB68B10F2822B97A (ProtoSerializer_tFEA7AC4576A3AF4078DF2F150739086926FFE43E* __this, RuntimeObject* ___0_value, ProtoWriter_tB8E54CFB9AA58B570DD20AB90ED11B461D4F90D8* ___1_dest, const RuntimeMethod* method) 
{
	typedef void (*FunctionPointerType) (RuntimeObject*, RuntimeObject*, ProtoWriter_tB8E54CFB9AA58B570DD20AB90ED11B461D4F90D8*, const RuntimeMethod*);
	((FunctionPointerType)__this->___invoke_impl)((Il2CppObject*)__this->___method_code, ___0_value, ___1_dest, reinterpret_cast<RuntimeMethod*>(__this->___method));
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* ProtoSerializer_BeginInvoke_mC298B1E2B6C9852802E61FCB87E276148683182E (ProtoSerializer_tFEA7AC4576A3AF4078DF2F150739086926FFE43E* __this, RuntimeObject* ___0_value, ProtoWriter_tB8E54CFB9AA58B570DD20AB90ED11B461D4F90D8* ___1_dest, AsyncCallback_t7FEF460CBDCFB9C5FA2EF776984778B9A4145F4C* ___2_callback, RuntimeObject* ___3_object, const RuntimeMethod* method) 
{
	void *__d_args[3] = {0};
	__d_args[0] = ___0_value;
	__d_args[1] = ___1_dest;
	return (RuntimeObject*)il2cpp_codegen_delegate_begin_invoke((RuntimeDelegate*)__this, __d_args, (RuntimeDelegate*)___2_callback, (RuntimeObject*)___3_object);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ProtoSerializer_EndInvoke_mCC0CF67EB779AE2162529E35C8C0E21D112AB06C (ProtoSerializer_tFEA7AC4576A3AF4078DF2F150739086926FFE43E* __this, RuntimeObject* ___0_result, const RuntimeMethod* method) 
{
	il2cpp_codegen_delegate_end_invoke((Il2CppAsyncResult*) ___0_result, 0);
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
RuntimeObject* ProtoDeserializer_Invoke_m1ED5FF4E6A2995C67653130E87D298A0228424DA_Multicast(ProtoDeserializer_t9C33607A0BAC6A3A9B5FFD51FE8B51EF52CA61E5* __this, RuntimeObject* ___0_value, ProtoReader_t894D20C36B7D7B81DC5345A22A8FF4E72F83AD53* ___1_source, const RuntimeMethod* method)
{
	il2cpp_array_size_t length = __this->___delegates->max_length;
	Delegate_t** delegatesToInvoke = reinterpret_cast<Delegate_t**>(__this->___delegates->GetAddressAtUnchecked(0));
	RuntimeObject* retVal = NULL;
	for (il2cpp_array_size_t i = 0; i < length; i++)
	{
		ProtoDeserializer_t9C33607A0BAC6A3A9B5FFD51FE8B51EF52CA61E5* currentDelegate = reinterpret_cast<ProtoDeserializer_t9C33607A0BAC6A3A9B5FFD51FE8B51EF52CA61E5*>(delegatesToInvoke[i]);
		typedef RuntimeObject* (*FunctionPointerType) (RuntimeObject*, RuntimeObject*, ProtoReader_t894D20C36B7D7B81DC5345A22A8FF4E72F83AD53*, const RuntimeMethod*);
		retVal = ((FunctionPointerType)currentDelegate->___invoke_impl)((Il2CppObject*)currentDelegate->___method_code, ___0_value, ___1_source, reinterpret_cast<RuntimeMethod*>(currentDelegate->___method));
	}
	return retVal;
}
RuntimeObject* ProtoDeserializer_Invoke_m1ED5FF4E6A2995C67653130E87D298A0228424DA_OpenInst(ProtoDeserializer_t9C33607A0BAC6A3A9B5FFD51FE8B51EF52CA61E5* __this, RuntimeObject* ___0_value, ProtoReader_t894D20C36B7D7B81DC5345A22A8FF4E72F83AD53* ___1_source, const RuntimeMethod* method)
{
	NullCheck(___0_value);
	typedef RuntimeObject* (*FunctionPointerType) (RuntimeObject*, ProtoReader_t894D20C36B7D7B81DC5345A22A8FF4E72F83AD53*, const RuntimeMethod*);
	return ((FunctionPointerType)__this->___method_ptr)(___0_value, ___1_source, method);
}
RuntimeObject* ProtoDeserializer_Invoke_m1ED5FF4E6A2995C67653130E87D298A0228424DA_OpenStatic(ProtoDeserializer_t9C33607A0BAC6A3A9B5FFD51FE8B51EF52CA61E5* __this, RuntimeObject* ___0_value, ProtoReader_t894D20C36B7D7B81DC5345A22A8FF4E72F83AD53* ___1_source, const RuntimeMethod* method)
{
	typedef RuntimeObject* (*FunctionPointerType) (RuntimeObject*, ProtoReader_t894D20C36B7D7B81DC5345A22A8FF4E72F83AD53*, const RuntimeMethod*);
	return ((FunctionPointerType)__this->___method_ptr)(___0_value, ___1_source, method);
}
RuntimeObject* ProtoDeserializer_Invoke_m1ED5FF4E6A2995C67653130E87D298A0228424DA_OpenVirtual(ProtoDeserializer_t9C33607A0BAC6A3A9B5FFD51FE8B51EF52CA61E5* __this, RuntimeObject* ___0_value, ProtoReader_t894D20C36B7D7B81DC5345A22A8FF4E72F83AD53* ___1_source, const RuntimeMethod* method)
{
	NullCheck(___0_value);
	return VirtualFuncInvoker1< RuntimeObject*, ProtoReader_t894D20C36B7D7B81DC5345A22A8FF4E72F83AD53* >::Invoke(il2cpp_codegen_method_get_slot(method), ___0_value, ___1_source);
}
RuntimeObject* ProtoDeserializer_Invoke_m1ED5FF4E6A2995C67653130E87D298A0228424DA_OpenInterface(ProtoDeserializer_t9C33607A0BAC6A3A9B5FFD51FE8B51EF52CA61E5* __this, RuntimeObject* ___0_value, ProtoReader_t894D20C36B7D7B81DC5345A22A8FF4E72F83AD53* ___1_source, const RuntimeMethod* method)
{
	NullCheck(___0_value);
	return InterfaceFuncInvoker1< RuntimeObject*, ProtoReader_t894D20C36B7D7B81DC5345A22A8FF4E72F83AD53* >::Invoke(il2cpp_codegen_method_get_slot(method), il2cpp_codegen_method_get_declaring_type(method), ___0_value, ___1_source);
}
RuntimeObject* ProtoDeserializer_Invoke_m1ED5FF4E6A2995C67653130E87D298A0228424DA_OpenGenericVirtual(ProtoDeserializer_t9C33607A0BAC6A3A9B5FFD51FE8B51EF52CA61E5* __this, RuntimeObject* ___0_value, ProtoReader_t894D20C36B7D7B81DC5345A22A8FF4E72F83AD53* ___1_source, const RuntimeMethod* method)
{
	NullCheck(___0_value);
	return GenericVirtualFuncInvoker1< RuntimeObject*, ProtoReader_t894D20C36B7D7B81DC5345A22A8FF4E72F83AD53* >::Invoke(method, ___0_value, ___1_source);
}
RuntimeObject* ProtoDeserializer_Invoke_m1ED5FF4E6A2995C67653130E87D298A0228424DA_OpenGenericInterface(ProtoDeserializer_t9C33607A0BAC6A3A9B5FFD51FE8B51EF52CA61E5* __this, RuntimeObject* ___0_value, ProtoReader_t894D20C36B7D7B81DC5345A22A8FF4E72F83AD53* ___1_source, const RuntimeMethod* method)
{
	NullCheck(___0_value);
	return GenericInterfaceFuncInvoker1< RuntimeObject*, ProtoReader_t894D20C36B7D7B81DC5345A22A8FF4E72F83AD53* >::Invoke(method, ___0_value, ___1_source);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ProtoDeserializer__ctor_m2324D59BB60493D69802BCD716E5976B79162D2A (ProtoDeserializer_t9C33607A0BAC6A3A9B5FFD51FE8B51EF52CA61E5* __this, RuntimeObject* ___0_object, intptr_t ___1_method, const RuntimeMethod* method) 
{
	__this->___method_ptr = (intptr_t)il2cpp_codegen_get_method_pointer((RuntimeMethod*)___1_method);
	__this->___method = ___1_method;
	__this->___m_target = ___0_object;
	Il2CppCodeGenWriteBarrier((void**)(&__this->___m_target), (void*)___0_object);
	int parameterCount = il2cpp_codegen_method_parameter_count((RuntimeMethod*)___1_method);
	__this->___method_code = (intptr_t)__this;
	if (MethodIsStatic((RuntimeMethod*)___1_method))
	{
		bool isOpen = parameterCount == 2;
		if (isOpen)
			__this->___invoke_impl = (intptr_t)&ProtoDeserializer_Invoke_m1ED5FF4E6A2995C67653130E87D298A0228424DA_OpenStatic;
		else
			{
				__this->___invoke_impl = __this->___method_ptr;
				__this->___method_code = (intptr_t)__this->___m_target;
			}
	}
	else
	{
		bool isOpen = parameterCount == 1;
		if (isOpen)
		{
			if (__this->___method_is_virtual)
			{
				if (il2cpp_codegen_method_is_generic_instance_method((RuntimeMethod*)___1_method))
					if (il2cpp_codegen_method_is_interface_method((RuntimeMethod*)___1_method))
						__this->___invoke_impl = (intptr_t)&ProtoDeserializer_Invoke_m1ED5FF4E6A2995C67653130E87D298A0228424DA_OpenGenericInterface;
					else
						__this->___invoke_impl = (intptr_t)&ProtoDeserializer_Invoke_m1ED5FF4E6A2995C67653130E87D298A0228424DA_OpenGenericVirtual;
				else
					if (il2cpp_codegen_method_is_interface_method((RuntimeMethod*)___1_method))
						__this->___invoke_impl = (intptr_t)&ProtoDeserializer_Invoke_m1ED5FF4E6A2995C67653130E87D298A0228424DA_OpenInterface;
					else
						__this->___invoke_impl = (intptr_t)&ProtoDeserializer_Invoke_m1ED5FF4E6A2995C67653130E87D298A0228424DA_OpenVirtual;
			}
			else
			{
				__this->___invoke_impl = (intptr_t)&ProtoDeserializer_Invoke_m1ED5FF4E6A2995C67653130E87D298A0228424DA_OpenInst;
			}
		}
		else
		{
			if (___0_object == NULL)
				il2cpp_codegen_raise_exception(il2cpp_codegen_get_argument_exception(NULL, "Delegate to an instance method cannot have null 'this'."), NULL);
			__this->___invoke_impl = __this->___method_ptr;
			__this->___method_code = (intptr_t)__this->___m_target;
		}
	}
	__this->___extra_arg = (intptr_t)&ProtoDeserializer_Invoke_m1ED5FF4E6A2995C67653130E87D298A0228424DA_Multicast;
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* ProtoDeserializer_Invoke_m1ED5FF4E6A2995C67653130E87D298A0228424DA (ProtoDeserializer_t9C33607A0BAC6A3A9B5FFD51FE8B51EF52CA61E5* __this, RuntimeObject* ___0_value, ProtoReader_t894D20C36B7D7B81DC5345A22A8FF4E72F83AD53* ___1_source, const RuntimeMethod* method) 
{
	typedef RuntimeObject* (*FunctionPointerType) (RuntimeObject*, RuntimeObject*, ProtoReader_t894D20C36B7D7B81DC5345A22A8FF4E72F83AD53*, const RuntimeMethod*);
	return ((FunctionPointerType)__this->___invoke_impl)((Il2CppObject*)__this->___method_code, ___0_value, ___1_source, reinterpret_cast<RuntimeMethod*>(__this->___method));
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* ProtoDeserializer_BeginInvoke_mC083DA01CB70C44FDC1BA57622C65FCBD1DF39EC (ProtoDeserializer_t9C33607A0BAC6A3A9B5FFD51FE8B51EF52CA61E5* __this, RuntimeObject* ___0_value, ProtoReader_t894D20C36B7D7B81DC5345A22A8FF4E72F83AD53* ___1_source, AsyncCallback_t7FEF460CBDCFB9C5FA2EF776984778B9A4145F4C* ___2_callback, RuntimeObject* ___3_object, const RuntimeMethod* method) 
{
	void *__d_args[3] = {0};
	__d_args[0] = ___0_value;
	__d_args[1] = ___1_source;
	return (RuntimeObject*)il2cpp_codegen_delegate_begin_invoke((RuntimeDelegate*)__this, __d_args, (RuntimeDelegate*)___2_callback, (RuntimeObject*)___3_object);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RuntimeObject* ProtoDeserializer_EndInvoke_m8B91FCF7A996FC5D63FC9EEB28DDED04A1AE2635 (ProtoDeserializer_t9C33607A0BAC6A3A9B5FFD51FE8B51EF52CA61E5* __this, RuntimeObject* ___0_result, const RuntimeMethod* method) 
{
	RuntimeObject *__result = il2cpp_codegen_delegate_end_invoke((Il2CppAsyncResult*) ___0_result, 0);
	return (RuntimeObject*)__result;
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Local__ctor_mF48224DF5E9358C792AC309D0E9EF1C94EEC6525 (Local_tD3297DE40276C3DB9E44DABFB2DF7A8C4477557B* __this, LocalBuilder_tCB5AF342391604CED7394B81A931AB5BC6B97BB6* ___0_value, Type_t* ___1_type, const RuntimeMethod* method) 
{
	{
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2(__this, NULL);
		LocalBuilder_tCB5AF342391604CED7394B81A931AB5BC6B97BB6* L_0 = ___0_value;
		__this->___value = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___value), (void*)L_0);
		Type_t* L_1 = ___1_type;
		__this->___type = L_1;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___type), (void*)L_1);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Local__ctor_mA3FAAB90E8C52DFEA913667FACB9E44DC3AE5E19 (Local_tD3297DE40276C3DB9E44DABFB2DF7A8C4477557B* __this, CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8* ___0_ctx, Type_t* ___1_type, const RuntimeMethod* method) 
{
	{
		Object__ctor_mE837C6B9FA8C6D5D109F4B2EC885D79919AC0EA2(__this, NULL);
		CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8* L_0 = ___0_ctx;
		__this->___ctx = L_0;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___ctx), (void*)L_0);
		CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8* L_1 = ___0_ctx;
		if (!L_1)
		{
			goto IL_001d;
		}
	}
	{
		CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8* L_2 = ___0_ctx;
		Type_t* L_3 = ___1_type;
		NullCheck(L_2);
		LocalBuilder_tCB5AF342391604CED7394B81A931AB5BC6B97BB6* L_4;
		L_4 = CompilerContext_GetFromPool_mF0FEA5084EACBC292A9FBE5E4A6D1229A34756F2(L_2, L_3, NULL);
		__this->___value = L_4;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___value), (void*)L_4);
	}

IL_001d:
	{
		Type_t* L_5 = ___1_type;
		__this->___type = L_5;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___type), (void*)L_5);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR LocalBuilder_tCB5AF342391604CED7394B81A931AB5BC6B97BB6* Local_get_Value_m22B518C4BE370270903E843E59051D77439DCD46 (Local_tD3297DE40276C3DB9E44DABFB2DF7A8C4477557B* __this, const RuntimeMethod* method) 
{
	LocalBuilder_tCB5AF342391604CED7394B81A931AB5BC6B97BB6* G_B2_0 = NULL;
	LocalBuilder_tCB5AF342391604CED7394B81A931AB5BC6B97BB6* G_B1_0 = NULL;
	{
		LocalBuilder_tCB5AF342391604CED7394B81A931AB5BC6B97BB6* L_0 = __this->___value;
		LocalBuilder_tCB5AF342391604CED7394B81A931AB5BC6B97BB6* L_1 = L_0;
		if (L_1)
		{
			G_B2_0 = L_1;
			goto IL_001b;
		}
		G_B1_0 = L_1;
	}
	{
		Type_t* L_2;
		L_2 = Object_GetType_mE10A8FC1E57F3DF29972CCBC026C2DC3942263B3(__this, NULL);
		NullCheck(L_2);
		String_t* L_3;
		L_3 = VirtualFuncInvoker0< String_t* >::Invoke(23, L_2);
		ObjectDisposedException_tC5FB29E8E980E2010A2F6A5B9B791089419F89EB* L_4 = (ObjectDisposedException_tC5FB29E8E980E2010A2F6A5B9B791089419F89EB*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ObjectDisposedException_tC5FB29E8E980E2010A2F6A5B9B791089419F89EB_il2cpp_TypeInfo_var)));
		ObjectDisposedException__ctor_mB2C8582279AF3F0C1CF9AA52DA7331BF848DFD48(L_4, L_3, NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_4, ((RuntimeMethod*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Local_get_Value_m22B518C4BE370270903E843E59051D77439DCD46_RuntimeMethod_var)));
	}

IL_001b:
	{
		return G_B2_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Type_t* Local_get_Type_m770B7E893A9310B76D58CAB864EF51701C05FB94 (Local_tD3297DE40276C3DB9E44DABFB2DF7A8C4477557B* __this, const RuntimeMethod* method) 
{
	{
		Type_t* L_0 = __this->___type;
		return L_0;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Local_tD3297DE40276C3DB9E44DABFB2DF7A8C4477557B* Local_AsCopy_m53F2962C71F3C58FC4A669770A67AB612DA49BE0 (Local_tD3297DE40276C3DB9E44DABFB2DF7A8C4477557B* __this, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Local_tD3297DE40276C3DB9E44DABFB2DF7A8C4477557B_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	{
		CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8* L_0 = __this->___ctx;
		if (L_0)
		{
			goto IL_000a;
		}
	}
	{
		return __this;
	}

IL_000a:
	{
		LocalBuilder_tCB5AF342391604CED7394B81A931AB5BC6B97BB6* L_1 = __this->___value;
		Type_t* L_2 = __this->___type;
		Local_tD3297DE40276C3DB9E44DABFB2DF7A8C4477557B* L_3 = (Local_tD3297DE40276C3DB9E44DABFB2DF7A8C4477557B*)il2cpp_codegen_object_new(Local_tD3297DE40276C3DB9E44DABFB2DF7A8C4477557B_il2cpp_TypeInfo_var);
		Local__ctor_mF48224DF5E9358C792AC309D0E9EF1C94EEC6525(L_3, L_1, L_2, NULL);
		return L_3;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Local_Dispose_mAD6A2620DF85A0E9928CABCF4552FAEF18A97191 (Local_tD3297DE40276C3DB9E44DABFB2DF7A8C4477557B* __this, const RuntimeMethod* method) 
{
	{
		CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8* L_0 = __this->___ctx;
		if (!L_0)
		{
			goto IL_0027;
		}
	}
	{
		CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8* L_1 = __this->___ctx;
		LocalBuilder_tCB5AF342391604CED7394B81A931AB5BC6B97BB6* L_2 = __this->___value;
		NullCheck(L_1);
		CompilerContext_ReleaseToPool_m5B36FA843A95DE38B8D7A1627D11CE98F48B1327(L_1, L_2, NULL);
		__this->___value = (LocalBuilder_tCB5AF342391604CED7394B81A931AB5BC6B97BB6*)NULL;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___value), (void*)(LocalBuilder_tCB5AF342391604CED7394B81A931AB5BC6B97BB6*)NULL);
		__this->___ctx = (CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8*)NULL;
		Il2CppCodeGenWriteBarrier((void**)(&__this->___ctx), (void*)(CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8*)NULL);
	}

IL_0027:
	{
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Local_IsSame_mDBB3507B61644ACB6419BC7376DBCBE528DD540C (Local_tD3297DE40276C3DB9E44DABFB2DF7A8C4477557B* __this, Local_tD3297DE40276C3DB9E44DABFB2DF7A8C4477557B* ___0_other, const RuntimeMethod* method) 
{
	RuntimeObject* V_0 = NULL;
	{
		Local_tD3297DE40276C3DB9E44DABFB2DF7A8C4477557B* L_0 = ___0_other;
		if ((!(((RuntimeObject*)(Local_tD3297DE40276C3DB9E44DABFB2DF7A8C4477557B*)__this) == ((RuntimeObject*)(Local_tD3297DE40276C3DB9E44DABFB2DF7A8C4477557B*)L_0))))
		{
			goto IL_0006;
		}
	}
	{
		return (bool)1;
	}

IL_0006:
	{
		LocalBuilder_tCB5AF342391604CED7394B81A931AB5BC6B97BB6* L_1 = __this->___value;
		V_0 = L_1;
		Local_tD3297DE40276C3DB9E44DABFB2DF7A8C4477557B* L_2 = ___0_other;
		if (!L_2)
		{
			goto IL_001a;
		}
	}
	{
		RuntimeObject* L_3 = V_0;
		Local_tD3297DE40276C3DB9E44DABFB2DF7A8C4477557B* L_4 = ___0_other;
		NullCheck(L_4);
		LocalBuilder_tCB5AF342391604CED7394B81A931AB5BC6B97BB6* L_5 = L_4->___value;
		return (bool)((((RuntimeObject*)(RuntimeObject*)L_3) == ((RuntimeObject*)(LocalBuilder_tCB5AF342391604CED7394B81A931AB5BC6B97BB6*)L_5))? 1 : 0);
	}

IL_001a:
	{
		return (bool)0;
	}
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR Type_t* Local_get_Type_m770B7E893A9310B76D58CAB864EF51701C05FB94_inline (Local_tD3297DE40276C3DB9E44DABFB2DF7A8C4477557B* __this, const RuntimeMethod* method) 
{
	{
		Type_t* L_0 = __this->___type;
		return L_0;
	}
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t CompilerContext_get_MetadataVersion_mE8212D3B0AE87B6D46E31B2A42FE196C2D7A1DAE_inline (CompilerContext_t82AF2E9E11A45F0AF55470966069E529AA3A95D8* __this, const RuntimeMethod* method) 
{
	{
		int32_t L_0 = __this->___metadataVersion;
		return L_0;
	}
}
