// ﻿﻿Copyright (c) Code Impressions, LLC. All Rights Reserved.
//  
//  Licensed under the Apache License, Version 2.0 (the "License")
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.

using Microsoft.AspNetCore.Mvc;

namespace Tandely.Customers.Service.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class CustomersController(CustomerRepository customerRepository, ILogger<CustomersController> logger) : ControllerBase
	{
		private readonly CustomerRepository _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
		private readonly ILogger<CustomersController> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

		[HttpGet("all")]
		public IEnumerable<Customer> AlLCustomers()
		{
			_logger.LogDebug("Get All Customers");

			return _customerRepository.GetAllUsers();
		}

		/// <summary>
		/// Get a customer by Id
		/// </summary>
		/// <param name="id" example="57eea4ca-3713-7441-7208-a2baded5c466">Id of the customer</param>
		/// <returns>Customer; otherwise null</returns>
		[HttpGet("{id}")]
		public Customer? GetCustomer(Guid id)
		{
			_logger.LogDebug("Get Customer");

			return _customerRepository.GetCustomer(id);
		}
	}
}