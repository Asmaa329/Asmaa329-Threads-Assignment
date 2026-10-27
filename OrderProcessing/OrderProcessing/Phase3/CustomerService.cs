using OrderProcessing.OrderProcessing.Phase1;
using OrderProcessing.OrderProcessing.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderProcessing.OrderProcessing.Phase3
{
    public class CustomerService
    {
        private readonly CustomerRepository _customerRepo;
        private readonly JsonFileStore _fileStore;
        public CustomerService(JsonFileStore fileStore)
        {
            _fileStore = fileStore;
            _customerRepo = new CustomerRepository(fileStore); 
        }
        public async Task<Customer> GetCustomerAsync(int id, CancellationToken ct)
        {
            var customer = await _customerRepo.GetByIdAsync(id, ct);
            if (customer == null)
            {
                throw new Exception($"Customer #{id} not found");
            }
            return customer;
        }
    }
}

