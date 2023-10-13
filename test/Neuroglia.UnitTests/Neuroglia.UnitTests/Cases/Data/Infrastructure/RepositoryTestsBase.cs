﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Neuroglia.Data.Infrastructure.Services;

namespace Neuroglia.UnitTests.Cases.Data.Infrastructure;

public abstract class RepositoryTestsBase 
    : IAsyncLifetime
{

    public RepositoryTestsBase(IServiceCollection services) { this.ServiceProvider = services.BuildServiceProvider(); }

    protected ServiceProvider ServiceProvider { get; }

    protected CancellationTokenSource CancellationTokenSource { get; } = new();

    protected virtual IRepository<User, string> Repository { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        foreach (var hostedService in this.ServiceProvider.GetServices<IHostedService>())
        {
            await hostedService.StartAsync(CancellationTokenSource.Token).ConfigureAwait(false);
        }
        this.Repository = this.ServiceProvider.GetRequiredService<IRepository<User, string>>();
    }

    public async Task DisposeAsync() => await ServiceProvider.DisposeAsync().ConfigureAwait(false);

    [Fact, Priority(1)]
    public async Task Add_Should_Work()
    {
        //arrange
        var user = User.Create();

        //act
        var result = await Repository.AddAsync(user);
        await Repository.SaveChangesAsync();

        //assert
        result.Should().NotBeNull();
        result.Id.Should().Be(user.Id);
        result.FirstName.Should().Be(user.FirstName);
        result.LastName.Should().Be(user.LastName);
        result.Email.Should().Be(user.Email);
    }

    [Fact, Priority(2)]
    public async Task Contains_Should_Work()
    {
        //arrange
        var user = await Repository.AddAsync(User.Create());
        await Repository.SaveChangesAsync();

        //act
        var result = await Repository.ContainsAsync(user.Id);

        //assert
        result.Should().BeTrue();
    }

    [Fact, Priority(3)]
    public async Task Get_Should_Work()
    {
        //arrange
        var user = await Repository.AddAsync(User.Create());
        await Repository.SaveChangesAsync();

        //act
        var result = await Repository.GetAsync(user.Id);

        //assert
        result.Should().NotBeNull();
        result!.FirstName.Should().Be(user.FirstName);
        result.LastName.Should().Be(user.LastName);
        result.Email?.Should().Be(user.Email);
    }

    [Fact, Priority(4)]
    public async Task Update_Should_Work()
    {
        //arrange
        var user = User.Create();
        await Repository.AddAsync(user);

        //act
        user.VerifyEmail();
        var result = await Repository.UpdateAsync(user);
        await Repository.SaveChangesAsync();

        //assert
        result.Should().NotBeNull();
        result.EmailVerified.Should().BeTrue();
    }

    [Fact, Priority(5)]
    public async Task Remove_Should_Work()
    {
        //arrange
        var user = await Repository.AddAsync(User.Create());
        await Repository.SaveChangesAsync();

        //act
        var result = await Repository.RemoveAsync(user.Id);
        var contains = await Repository.ContainsAsync(user.Id);

        //assert
        result.Should().BeTrue();
        contains.Should().BeFalse();
    }

}
