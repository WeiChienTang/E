using ERPCore2.Data.Entities;
using ERPCore2.Repositories.Interfaces;
using ERPCore2.Services.Models;

namespace ERPCore2.Services
{
    public class CustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ICustomerTypeRepository _customerTypeRepository;
        private readonly IIndustryRepository _industryRepository;
        
        public CustomerService(
            ICustomerRepository customerRepository,
            ICustomerTypeRepository customerTypeRepository,
            IIndustryRepository industryRepository)
        {
            _customerRepository = customerRepository;
            _customerTypeRepository = customerTypeRepository;
            _industryRepository = industryRepository;
        }
        
        public async Task<List<Customer>> GetAllAsync()
        {
            return await _customerRepository.GetAllAsync();
        }
        
        public async Task<List<Customer>> GetAllWithDetailsAsync()
        {
            return await _customerRepository.GetAllWithDetailsAsync();
        }
        
        public async Task<Customer?> GetByIdAsync(int id)
        {
            if (id <= 0)
                return null;
                
            return await _customerRepository.GetByIdAsync(id);
        }
        
        public async Task<Customer?> GetByIdWithDetailsAsync(int id)
        {
            if (id <= 0)
                return null;
                
            return await _customerRepository.GetByIdWithDetailsAsync(id);
        }
        
        public async Task<ServiceResult<Customer>> CreateAsync(CreateCustomerRequest request)
        {
            // Business Validation
            var validationResult = await ValidateCreateRequestAsync(request);
            if (!validationResult.IsSuccess)
                return ServiceResult<Customer>.Failure(validationResult.ErrorMessage);
            
            // Business Rules - Check for duplicate customer code
            var existingCustomer = await _customerRepository.GetByCustomerCodeAsync(request.CustomerCode);
            if (existingCustomer != null)
                return ServiceResult<Customer>.Failure("客戶代碼已存在");
            
            // Create Entity
            var customer = new Customer
            {
                CustomerCode = request.CustomerCode,
                CompanyName = request.CompanyName,
                ContactPerson = request.ContactPerson,
                TaxNumber = request.TaxNumber,
                CustomerTypeId = request.CustomerTypeId,
                IndustryId = request.IndustryId,
                CreatedBy = request.CreatedBy,
                Status = EntityStatus.Active
            };
            
            // Save
            var result = await _customerRepository.AddAsync(customer);
            
            return ServiceResult<Customer>.Success(result);
        }
        
        public async Task<ServiceResult<Customer>> UpdateAsync(UpdateCustomerRequest request)
        {
            // Business Validation
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId);
            if (customer == null)
                return ServiceResult<Customer>.Failure("客戶不存在");
            
            // Business Rules - Check for duplicate customer code (excluding current record)
            var existingCustomer = await _customerRepository.GetByCustomerCodeAsync(request.CustomerCode);
            if (existingCustomer != null && existingCustomer.CustomerId != request.CustomerId)
                return ServiceResult<Customer>.Failure("客戶代碼已存在");
            
            // Update Entity
            customer.CustomerCode = request.CustomerCode;
            customer.CompanyName = request.CompanyName;
            customer.ContactPerson = request.ContactPerson;
            customer.TaxNumber = request.TaxNumber;
            customer.CustomerTypeId = request.CustomerTypeId;
            customer.IndustryId = request.IndustryId;
            customer.Status = request.Status;
            customer.ModifiedBy = request.ModifiedBy;
            
            // Save
            var result = await _customerRepository.UpdateAsync(customer);
            
            return ServiceResult<Customer>.Success(result);
        }
        
        public async Task<ServiceResult> DeleteAsync(int id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null)
                return ServiceResult.Failure("客戶不存在");
            
            // Business Rules - Check for dependencies
            // TODO: Add business logic to check if customer has related records
            // that should prevent deletion
            
            await _customerRepository.DeleteAsync(id);
            
            return ServiceResult.Success();
        }
        
        public async Task<ServiceResult> ActivateAsync(int id, string modifiedBy)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null)
                return ServiceResult.Failure("客戶不存在");
                
            customer.Status = EntityStatus.Active;
            customer.ModifiedBy = modifiedBy;
            await _customerRepository.UpdateAsync(customer);
            
            return ServiceResult.Success();
        }
        
        public async Task<ServiceResult> DeactivateAsync(int id, string modifiedBy)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null)
                return ServiceResult.Failure("客戶不存在");
                
            customer.Status = EntityStatus.Inactive;
            customer.ModifiedBy = modifiedBy;
            await _customerRepository.UpdateAsync(customer);
            
            return ServiceResult.Success();
        }
        
        public async Task<List<Customer>> GetByCustomerTypeAsync(int customerTypeId)
        {
            return await _customerRepository.GetByCustomerTypeAsync(customerTypeId);
        }
        
        public async Task<List<Customer>> GetByIndustryAsync(int industryId)
        {
            return await _customerRepository.GetByIndustryAsync(industryId);
        }
        
        public async Task<(List<Customer> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;
            
            return await _customerRepository.GetPagedAsync(pageNumber, pageSize);
        }
          private Task<ServiceResult> ValidateCreateRequestAsync(CreateCustomerRequest request)
        {
            var errors = new List<string>();
            
            if (string.IsNullOrWhiteSpace(request.CustomerCode))
                errors.Add("客戶代碼為必填");
                
            if (string.IsNullOrWhiteSpace(request.CompanyName))
                errors.Add("公司名稱為必填");
            
            // Add more validation rules as needed
            if (request.CustomerCode?.Length > 20)
                errors.Add("客戶代碼不可超過20個字元");
                
            if (request.CompanyName?.Length > 100)
                errors.Add("公司名稱不可超過100個字元");
            
            if (errors.Any())
                return Task.FromResult(ServiceResult.ValidationFailure(errors));
                
            return Task.FromResult(ServiceResult.Success());
        }
        
        public async Task<List<CustomerType>> GetAllCustomerTypesAsync()
        {
            return await _customerTypeRepository.GetAllAsync();
        }
        
        public async Task<List<Industry>> GetAllIndustriesAsync()
        {
            return await _industryRepository.GetAllAsync();
        }
    }
}
