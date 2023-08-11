using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using FluffRest.Client;
using FluffRestBenchmark.Infra;
using FluffRestTest.Dto;
using System.Net.Http;
using System.Threading.Tasks;

namespace FluffRestBenchmark.Benchmarks
{
    [MemoryDiagnoser]
    public class BasicBenchmark : BaseBenchmark
    {
        private readonly IFluffRestClient _client;

        [Params(1, 5, 10, 20)]
        public int N { get; set; }

        public BasicBenchmark()
        {
            var dto = GetBasicDto();
            var url = $"{TestUrl}/benchmark";
            var json = System.Text.Json.JsonSerializer.Serialize(dto);
            var httpClient = GetMockedClient(url, JsonContentType, json, HttpMethod.Get);
            _client = new FluffRestClient(TestUrl, httpClient);
        }

        [Benchmark]
        public async Task ExecuteBasicRequestWithParametersAsync()
        {
            var request = _client.Get("benchmark");

            for (int i = 0; i < N; i++)
            {
                request.AddQueryParameter(i.ToString(), i);
            }

            await request.ExecAsync<TestUserDto>();
        }

        [Benchmark]
        public async Task ExecuteBasicRequestWithHeadersAsync()
        {
            var request = _client.Get("benchmark");

            for (int i = 0; i < N; i++)
            {
                request.AddHeader(i.ToString(), i.ToString());
            }

            await request.ExecAsync<TestUserDto>();
        }
    }
}
