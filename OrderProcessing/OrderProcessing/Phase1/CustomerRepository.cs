using OrderProcessing.OrderProcessing.Shared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace OrderProcessing.OrderProcessing.Phase1
{
    public class CustomerRepository
    {
        private readonly JsonFileStore _fileStore;

        public CustomerRepository(JsonFileStore fileStore)
        {
            _fileStore = fileStore;
        }
        public async Task<List<Customer>> GetAllAsync(CancellationToken ct)
        {
            Log.Info("CustomerRepository => Open connection");
            await Task.Delay(200,ct);
            Log.Info("CustomerRepository => Execute query");
            await Task.Delay(Random.Shared.Next(300, 601), ct);
            return await _fileStore.ReadAsync<List<Customer>>("customers.json", ct);

        }
        public async Task<Customer?> GetByIdAsync(int id, CancellationToken ct)
        {
            Log.Info("CustomerRepository => Open connection");
            await Task.Delay(200,ct);
            Log.Info("CustomerRepository => Execute query");
            await Task.Delay(Random.Shared.Next(300, 601), ct);
            var customerlist =   await _fileStore.ReadAsync<List<Customer>>("customers.json",ct);
            return customerlist.FirstOrDefault(c => c.Id == id);
        }
        public async Task SaveAsync(Customer customer, CancellationToken ct)
        {
            Log.Info("CustomerRepository => Open connection");
            await Task.Delay(200, ct);
            Log.Info("CustomerRepository => Execute query");
            await Task.Delay(Random.Shared.Next(300, 601), ct);
            var customerlist = await _fileStore.ReadAsync<List<Customer>>("customers.json", ct);
            customerlist.Add(customer);
            await _fileStore.WriteAsync<List<Customer>>("customers.json", customerlist ,ct);

        }


    } 
}

