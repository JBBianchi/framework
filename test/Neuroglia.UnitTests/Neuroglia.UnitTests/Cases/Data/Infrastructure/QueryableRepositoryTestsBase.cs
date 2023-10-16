﻿// Copyright © 2021-Present Neuroglia SRL. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License"),
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Microsoft.Extensions.DependencyInjection;
using Neuroglia.Data.Infrastructure.Services;

namespace Neuroglia.UnitTests.Cases.Data.Infrastructure;

public abstract class QueryableRepositoryTestsBase
    : RepositoryTestsBase
{

    protected QueryableRepositoryTestsBase(IServiceCollection services) : base(services) { }

    protected override IQueryableRepository<User, string> Repository => (IQueryableRepository<User, string>)base.Repository;

    [Fact, Priority(6)]
    public async Task Query_Should_Work()
    {
        //arrange
        var user = await Repository.AddAsync(User.Create());
        await Repository.SaveChangesAsync();

        //assert
        this.Repository.AsQueryable()
            .Where(u => u.FirstName == "John" && u.LastName == "Doe")
            .ToList()
            .Should()
            .NotBeNullOrEmpty();
    }

}