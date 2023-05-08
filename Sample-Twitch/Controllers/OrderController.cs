using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sample.Contracts;

namespace Sample_Twitch.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly ILogger<OrderController> logger;
        private readonly IRequestClient<ISubmitOrder> submitOrderRequestClient;
        private readonly ISendEndpointProvider sendEndpointProvider;
        private readonly IRequestClient<ICheckOrder> checkOrderRequestClient;

        public OrderController(ILogger<OrderController> logger, 
            IRequestClient<ISubmitOrder> submitOrderRequestClient,
            IRequestClient<ICheckOrder> checkOrderRequestClient,
            ISendEndpointProvider sendEndpointProvider)
        {
            this.logger = logger;
            this.submitOrderRequestClient = submitOrderRequestClient;
            this.checkOrderRequestClient = checkOrderRequestClient;
            this.sendEndpointProvider = sendEndpointProvider;
        }

        [HttpGet]
        public async Task<IActionResult> Get(Guid id)
        {
            var (status, notFound) = await this.checkOrderRequestClient.GetResponse<IOrderStatus, IOrderNotFound>(new
            {
                OrderId = id
            });

            if(status.IsCompletedSuccessfully)
            {
                var response = await status;
                return Ok(response.Message);
            }
            else
            {
                var response = await notFound;
                return NotFound(response.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post(Guid id, string customerNumber)
        {
            var (accepted, rejected) = await submitOrderRequestClient.GetResponse<IOrderSubmissionAccepted, IOrderSubmissionRejected>(new
            {
                OrderId = id,
                InVar.Timestamp,
                CustomerNumber = customerNumber
            });

            if (accepted.IsCompletedSuccessfully)
            {
                var response = await accepted;
                return Accepted(response);
            }
            else
            {
                var response = await rejected;
                return BadRequest(response.Message);
            }
        }

        [HttpPut]
        public async Task<IActionResult> Put(Guid id, string customerNumber)
        {
            var endpoint = await this.sendEndpointProvider.GetSendEndpoint(new Uri("exchange:submit-order"));
            await endpoint.Send<ISubmitOrder>(new
            {
                OrderId = id,
                InVar.Timestamp,
                CustomerNumber = customerNumber
            });

            return Accepted();
        }
    }
}
