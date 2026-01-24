using FluffRest.Client;
using FluffRestTest.Dto;
using FluffRestTest.Infra;
using System.Net.Http;
using System;
using System.Threading.Tasks;
using FluffRest.Exception;
using FluffRest.Settings;
using System.Collections.Generic;

namespace FluffRestTest.Tests;

[TestClass]
public class DefaultParametersTests : BaseTest
{
    [TestMethod]
    public async Task BasicDefaultParameterTest()
    {
        // Arrange

        var dto = GetBasicDto();
        var url = $"{TestUrl}/simple?id=1&name=Test&short=2&long=3";
        var json = System.Text.Json.JsonSerializer.Serialize(dto);
        var httpClient = GetMockedClient(url, JsonContentType, json, HttpMethod.Get);
        var fluffClient = new FluffRestClient(TestUrl, httpClient)
            .AddDefaultQueryParameter("id", 1)
            .AddDefaultQueryParameter("name", "Test")
            .AddDefaultQueryParameter("short", (short)2)
            .AddDefaultQueryParameter("long", (long)3);

        // Act
        var result = await fluffClient.Get("simple").ExecAsync<TestUserDto>();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(result.Id, dto.Id);
        Assert.AreEqual(result.Name, dto.Name);
    }

    [TestMethod]
    public async Task DuplicateDefaultParameterThrow()
    {
        // Arrange
        TestUserDto result = null;
        var dto = GetBasicDto();

        try
        {
            var url = $"{TestUrl}/simple?id=1&name=Test";
            var json = System.Text.Json.JsonSerializer.Serialize(dto);
            var httpClient = GetMockedClient(url, JsonContentType, json, HttpMethod.Get);
            var fluffClient = new FluffRestClient(TestUrl, httpClient)
                .AddDefaultQueryParameter("id", 1)
                .AddDefaultQueryParameter("name", "Test")
                .AddDefaultQueryParameter("name", "Test");

            // Act
            result = await fluffClient.Get("simple").ExecAsync<TestUserDto>();
            
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Id, dto.Id);
            Assert.AreEqual(result.Name, dto.Name);
        }
        catch (Exception ex)
        {
            Assert.IsInstanceOfType<FluffDuplicateParameterException>(ex);
        }
    }

    [TestMethod]
    public async Task DuplicateDefaultParameterIgnore()
    {
        // Arrange

        var dto = GetBasicDto();
        var url = $"{TestUrl}/simple?id=1&name=Test";
        var json = System.Text.Json.JsonSerializer.Serialize(dto);
        var httpClient = GetMockedClient(url, JsonContentType, json, HttpMethod.Get);
        var options = new FluffClientSettings(FluffDuplicateParameterKeyHandling.Ignore);
        var fluffClient = new FluffRestClient(TestUrl, httpClient, options)
            .AddDefaultQueryParameter("id", 1)
            .AddDefaultQueryParameter("name", "Test")
            .AddDefaultQueryParameter("name", "Test");

        // Act
        var result = await fluffClient.Get("simple").ExecAsync<TestUserDto>();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(result.Id, dto.Id);
        Assert.AreEqual(result.Name, dto.Name);
    }

    [TestMethod]
    public async Task DuplicateParameterReplace()
    {
        // Arrange

        var dto = GetBasicDto();
        var url = $"{TestUrl}/simple?id=1&name=Test";
        var json = System.Text.Json.JsonSerializer.Serialize(dto);
        var httpClient = GetMockedClient(url, JsonContentType, json, HttpMethod.Get);
        var options = new FluffClientSettings(FluffDuplicateParameterKeyHandling.Replace);
        var fluffClient = new FluffRestClient(TestUrl, httpClient, options)
            .AddDefaultQueryParameter("id", 1)
            .AddDefaultQueryParameter("name", "Baba")
            .AddDefaultQueryParameter("name", "Test");

        // Act
        var result = await fluffClient.Get("simple").ExecAsync<TestUserDto>();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(result.Id, dto.Id);
        Assert.AreEqual(result.Name, dto.Name);
    }

    [TestMethod]
    public async Task PreventDefaultParametersFromBeingUsedAsDefaultParameterListAsync()
    {
        // Arrange
        var dto = GetBasicDto();
        var url = $"{TestUrl}/simple?id=1";
        var json = System.Text.Json.JsonSerializer.Serialize(dto);
        var httpClient = GetMockedClient(url, JsonContentType, json, HttpMethod.Get);
        var options = new FluffClientSettings(FluffDuplicateParameterKeyHandling.Throw);
        var fluffClient = new FluffRestClient(TestUrl, httpClient, options);

        List<TestUserDto> results = new List<TestUserDto>();

        // Act
        for (int i = 0; i < 10; i++)
        {
            var result = await fluffClient.Get("simple")
                .AddQueryParameter("id", 1)
                .ExecAsync<TestUserDto>();

            results.Add(result);
        }

        // Assert
        Assert.HasCount(10, results);
    }
}