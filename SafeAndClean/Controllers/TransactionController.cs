using Data.ConstData;
using Data.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SafeAndClean.Extensions;
using Services.Core;
using System;
using System.Collections.Generic;

namespace SafeAndClean.Controlleresult
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpGet("{id}")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public IActionResult GetGetById(Guid id)
        {
            var result = _transactionService.GetById(id);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpGet("User")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public IActionResult GetUserTransaction(Guid? bookingId, int pageIndex = 1, int pageSize = 10)
        {
            var result = _transactionService.GetByType(bookingId, ConstTransactionType.USER, pageIndex, pageSize);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        //[HttpGet("Booking")]
        //[Authorize(AuthenticationSchemes = "Bearer")]
        //public IActionResult GetBookingTransaction()
        //{
        //    var result = _transactionService.GetByType(ConstTransactionType.BOOKING);

        //    if (result.Succeed) return Ok(result.Data);
        //    return BadRequest(result.ErrorMessage);
        //}

        [HttpGet("Company")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public IActionResult GetCompanyTransaction(Guid? bookingId, int pageIndex = 1, int pageSize = 10)
        {
            var result = _transactionService.GetByType(bookingId, ConstTransactionType.COMPANY, pageIndex, pageSize);

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }
        
        [HttpGet("UserHistory")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public IActionResult GetUserHistory()
        {
            var result = _transactionService.GetUserHistory(User.GetId());

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

        [HttpPost("Deposit")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public IActionResult Deposit(double money)
        {
            var result = _transactionService.Deposit(money, User.GetId());

            if (result.Succeed) return Ok(result.Data);
            return BadRequest(result.ErrorMessage);
        }

    }
}
