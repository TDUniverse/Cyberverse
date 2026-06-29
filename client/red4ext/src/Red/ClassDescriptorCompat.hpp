#pragma once

#include <cstring>
#include <RedLib.hpp>

namespace Red
{
template<typename TClass>
class CyberverseClassDescriptorDefaultImpl : public ClassDescriptor<TClass>
{
    const bool IsEqual(const ScriptInstance aLhs, const ScriptInstance aRhs, uint32_t a3) final // 48
    {
        if constexpr (Detail::IsConstructionForwarded<TClass>)
        {
            if (!CClass::parent->flags.isAbstract)
            {
                return CClass::parent->IsEqual(aLhs, aRhs, a3);
            }
            return false;
        }
        else
        {
            using func_t = bool (*)(CClass*, const ScriptInstance, const ScriptInstance, uint32_t);
#if defined(RED4EXT_V1_SDK_VERSION_CURRENT) || defined(RED4EXT_SDK_0_5_0)
            static UniversalRelocFunc<func_t> func(RED4ext::Detail::AddressHashes::TTypedClass_IsEqual);
#else
            static RelocFunc<func_t> func(RED4ext::Addresses::TTypedClass_IsEqual);
#endif
            return func(this, aLhs, aRhs, a3);
        }
    }

    void Assign(ScriptInstance aLhs, const ScriptInstance aRhs) const final // 50
    {
        if constexpr (Detail::IsConstructionForwarded<TClass>)
        {
            if (!CClass::parent->flags.isAbstract)
            {
                CClass::parent->Assign(aLhs, aRhs);
            }
        }
        else if constexpr (std::is_copy_assignable_v<TClass>)
        {
            *reinterpret_cast<TClass*>(aLhs) = *reinterpret_cast<const TClass*>(aRhs);
        }
        else if constexpr (std::is_copy_constructible_v<TClass>)
        {
            reinterpret_cast<TClass*>(aLhs)->~TClass();
            new (aLhs) TClass(*reinterpret_cast<const TClass*>(aRhs));
        }
    }

    [[nodiscard]] Memory::IAllocator* GetAllocator() const final // B8
    {
        if constexpr (Detail::HasStaticAllocator<TClass>)
        {
            return Detail::ResolveAllocatorType<TClass>::Get();
        }
        else if constexpr (Detail::IsAllocationForwarded<TClass> || Detail::IsConstructionForwarded<TClass>)
        {
            return CClass::parent->GetAllocator();
        }
        else
        {
            return Memory::RTTIAllocator::Get();
        }
    }

    void ConstructCls(void* aMemory) const final // D8
    {
        if constexpr (Detail::IsConstructionForwarded<TClass>)
        {
            if (!CClass::parent->flags.isAbstract)
            {
                CClass::parent->ConstructCls(aMemory);
            }
        }
        else if constexpr (!std::is_abstract_v<TClass>)
        {
            new (aMemory) TClass();
        }
    }

    void DestructCls(void* aMemory) const final // E0
    {
        if constexpr (Detail::IsConstructionForwarded<TClass>)
        {
            if (!CClass::parent->flags.isAbstract)
            {
                CClass::parent->DestructCls(aMemory);
            }
        }
        else
        {
            reinterpret_cast<TClass*>(aMemory)->~TClass();
        }
    }

    [[nodiscard]] void* AllocMemory() const final // E8
    {
        auto classAlignment = CClass::GetAlignment();
        auto alignedSize = AlignUp(CClass::GetSize(), classAlignment);

        auto allocator = GetAllocator();
        auto allocResult = allocator->AllocAligned(alignedSize, classAlignment);

        std::memset(allocResult.memory, 0, allocResult.size);
        return allocResult.memory;
    }
};
} // namespace Red
