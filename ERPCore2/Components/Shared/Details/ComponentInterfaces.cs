using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;

namespace ERPCore2.Components.Shared.Details
{
    // 聯絡方式實體介面
    public interface IContactEntity
    {
        string? ContactValue { get; }
        bool IsPrimary { get; }
        bool? Status { get; }
        string? Remarks { get; }
        IContactTypeEntity? ContactType { get; }
    }

    // 聯絡方式類型實體介面
    public interface IContactTypeEntity
    {
        string TypeName { get; }
        int? SortOrder { get; }
    }

    // 地址實體介面
    public interface IAddressEntity
    {
        string? PostalCode { get; }
        string? City { get; }
        string? District { get; }
        string? Address { get; }
        bool IsPrimary { get; }
        bool? Status { get; }
        string? Remarks { get; }
        IAddressTypeEntity? AddressType { get; }
    }

    // 地址類型實體介面
    public interface IAddressTypeEntity
    {
        string TypeName { get; }
        int? SortOrder { get; }
    }

    // 適配器實作
    // CustomerContact 適配器
    public class CustomerContactAdapter : IContactEntity
    {
        private readonly CustomerContact _contact;

        public CustomerContactAdapter(CustomerContact contact)
        {
            _contact = contact;
        }

        public string? ContactValue => _contact.ContactValue;
        public bool IsPrimary => _contact.IsPrimary;
        public bool? Status => _contact.Status == EntityStatus.Active;
        public string? Remarks => _contact.Remarks;
        public IContactTypeEntity? ContactType => 
            _contact.ContactType != null ? new ContactTypeAdapter(_contact.ContactType) : null;
    }

    // ContactType 適配器
    public class ContactTypeAdapter : IContactTypeEntity
    {
        private readonly ContactType _contactType;

        public ContactTypeAdapter(ContactType contactType)
        {
            _contactType = contactType;
        }

        public string TypeName => _contactType.TypeName;
        public int? SortOrder => null; // ContactType 沒有 SortOrder 屬性
    }

    // CustomerAddress 適配器
    public class CustomerAddressAdapter : IAddressEntity
    {
        private readonly CustomerAddress _address;

        public CustomerAddressAdapter(CustomerAddress address)
        {
            _address = address;
        }

        public string? PostalCode => _address.PostalCode;
        public string? City => _address.City;
        public string? District => _address.District;
        public string? Address => _address.Address;
        public bool IsPrimary => _address.IsPrimary;
        public bool? Status => _address.Status == EntityStatus.Active;
        public string? Remarks => _address.Remarks;
        public IAddressTypeEntity? AddressType => 
            _address.AddressType != null ? new AddressTypeAdapter(_address.AddressType) : null;
    }

    // AddressType 適配器
    public class AddressTypeAdapter : IAddressTypeEntity
    {
        private readonly AddressType _addressType;

        public AddressTypeAdapter(AddressType addressType)
        {
            _addressType = addressType;
        }

        public string TypeName => _addressType.TypeName;
        public int? SortOrder => null; // AddressType 沒有 SortOrder 屬性
    }

    // SupplierContact 適配器
    public class SupplierContactAdapter : IContactEntity
    {
        private readonly SupplierContact _contact;

        public SupplierContactAdapter(SupplierContact contact)
        {
            _contact = contact;
        }

        public string? ContactValue => _contact.ContactValue;
        public bool IsPrimary => _contact.IsPrimary;
        public bool? Status => _contact.Status == EntityStatus.Active;
        public string? Remarks => _contact.Remarks;
        public IContactTypeEntity? ContactType => 
            _contact.ContactType != null ? new ContactTypeAdapter(_contact.ContactType) : null;
    }

    // SupplierAddress 適配器
    public class SupplierAddressAdapter : IAddressEntity
    {
        private readonly SupplierAddress _address;

        public SupplierAddressAdapter(SupplierAddress address)
        {
            _address = address;
        }

        public string? PostalCode => _address.PostalCode;
        public string? City => _address.City;
        public string? District => _address.District;
        public string? Address => _address.Address;
        public bool IsPrimary => _address.IsPrimary;
        public bool? Status => _address.Status == EntityStatus.Active;
        public string? Remarks => _address.Remarks;
        public IAddressTypeEntity? AddressType => 
            _address.AddressType != null ? new AddressTypeAdapter(_address.AddressType) : null;
    }

    // EmployeeContact 適配器
    public class EmployeeContactAdapter : IContactEntity
    {
        private readonly EmployeeContact _contact;

        public EmployeeContactAdapter(EmployeeContact contact)
        {
            _contact = contact;
        }

        public string? ContactValue => _contact.ContactValue;
        public bool IsPrimary => _contact.IsPrimary;
        public bool? Status => _contact.Status == EntityStatus.Active;
        public string? Remarks => _contact.Remarks;
        public IContactTypeEntity? ContactType => 
            _contact.ContactType != null ? new ContactTypeAdapter(_contact.ContactType) : null;
    }

    // EmployeeAddress 適配器
    public class EmployeeAddressAdapter : IAddressEntity
    {
        private readonly EmployeeAddress _address;

        public EmployeeAddressAdapter(EmployeeAddress address)
        {
            _address = address;
        }

        public string? PostalCode => _address.PostalCode;
        public string? City => _address.City;
        public string? District => _address.District;
        public string? Address => _address.Address;
        public bool IsPrimary => _address.IsPrimary;
        public bool? Status => _address.Status == EntityStatus.Active;
        public string? Remarks => _address.Remarks;
        public IAddressTypeEntity? AddressType => 
            _address.AddressType != null ? new AddressTypeAdapter(_address.AddressType) : null;
    }
}

// 擴展方法 - 放在不同的命名空間以避免衝突
namespace ERPCore2.Extensions
{
    using ERPCore2.Components.Shared.Details;

    public static class EntityAdapterExtensions
    {
        // 轉換 CustomerContact 集合為適配器集合
        public static IEnumerable<IContactEntity> AsContactDisplayEntities(
            this IEnumerable<CustomerContact> contacts)
        {
            return contacts.Select(c => new CustomerContactAdapter(c));
        }

        // 轉換 CustomerAddress 集合為適配器集合
        public static IEnumerable<IAddressEntity> AsAddressDisplayEntities(
            this IEnumerable<CustomerAddress> addresses)
        {
            return addresses.Select(a => new CustomerAddressAdapter(a));
        }

        // 轉換 SupplierContact 集合為適配器集合
        public static IEnumerable<IContactEntity> AsContactDisplayEntities(
            this IEnumerable<SupplierContact> contacts)
        {
            return contacts.Select(c => new SupplierContactAdapter(c));
        }

        // 轉換 SupplierAddress 集合為適配器集合
        public static IEnumerable<IAddressEntity> AsAddressDisplayEntities(
            this IEnumerable<SupplierAddress> addresses)
        {
            return addresses.Select(a => new SupplierAddressAdapter(a));
        }

        // 轉換 EmployeeContact 集合為適配器集合
        public static IEnumerable<IContactEntity> AsContactDisplayEntities(
            this IEnumerable<EmployeeContact> contacts)
        {
            return contacts.Select(c => new EmployeeContactAdapter(c));
        }

        // 轉換 EmployeeAddress 集合為適配器集合
        public static IEnumerable<IAddressEntity> AsAddressDisplayEntities(
            this IEnumerable<EmployeeAddress> addresses)
        {
            return addresses.Select(a => new EmployeeAddressAdapter(a));
        }
    }
}
