﻿using Neuroglia.Data;
using Neuroglia.Mediation;
using Neuroglia.UnitTests.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Neuroglia.UnitTests.Commands
{

    [PipelineMiddleware(typeof(DomainExceptionHandlingMiddleware<,>))]
    public class TestCommandWithDomainExceptionHandlingMiddleware
        : Command
    {

        public TestCommandWithDomainExceptionHandlingMiddleware(TestPerson person)
        {
            this.Person = person;
        }

        public TestPerson Person { get; }

    }

    public class TestCommandWithDomainExceptionHandlingMiddlewareHandler
        : ICommandHandler<TestCommandWithDomainExceptionHandlingMiddleware>
    {

        public Task<IOperationResult> HandleAsync(TestCommandWithDomainExceptionHandlingMiddleware request, CancellationToken cancellationToken = default)
        {
            throw DomainException.ArgumentNull("fake");
        }

    }
}
