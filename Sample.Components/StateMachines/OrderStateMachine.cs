﻿using MassTransit;
using Sample.Contracts;

namespace Sample.Components.StateMachines
{
    public class OrderStateMachine : MassTransitStateMachine<OrderState>
    {
        public OrderStateMachine()
        {
            Event(() => OrderSubmitted, x => x.CorrelateById(m => m.Message.OrderId));
            Event(() => OrderStatuRequested, x =>
            {
                x.CorrelateById(m => m.Message.OrderId);
                x.OnMissingInstance(m => m.ExecuteAsync(async context =>
                {
                    if(context.RequestId.HasValue)
                    {
                        await context.RespondAsync<IOrderNotFound>(new { context.Message.OrderId });
                    }
                }));
            });

            InstanceState(x => x.CurrentState);

            Initially(
                When(OrderSubmitted)
                .Then(context =>
                {
                    context.Saga.SubmitDate = context.Message.Timestamp;
                    context.Saga.CustomerNumber = context.Message.CustomerNumber;
                    context.Saga.Updated = DateTime.UtcNow;
                })
                .TransitionTo(Submitted));

            During(Submitted, Ignore(OrderSubmitted));

            DuringAny(
                When(OrderSubmitted)
                    .Then(context =>
                    {
                        context.Saga.SubmitDate ??= context.Message.Timestamp;
                        context.Saga.CustomerNumber ??= context.Message.CustomerNumber;
                    })
                );

            DuringAny(
                When(OrderStatuRequested)
                .RespondAsync(x => x.Init<IOrderStatus>(new
                {
                    OrderId = x.Saga.CorrelationId,
                    State = x.Saga.CurrentState
                })));
        }

        public State Submitted { get; private set; }

        public Event<IOrderSubmitted> OrderSubmitted { get; private set; }
        public Event<ICheckOrder> OrderStatuRequested { get; private set; }
    }

    public class OrderState : SagaStateMachineInstance, ISagaVersion
    {
        public Guid CorrelationId { get; set; }
        public int Version { get; set; }

        public string CurrentState { get; set; }

        public string CustomerNumber { get; set; }

        public DateTime? SubmitDate { get; set; }
        public DateTime? Updated { get; set; }
    }
}
