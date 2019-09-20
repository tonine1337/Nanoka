# nanoka-idx

This folder contains the Nanoka backend source code, C# client library and various extension projects.

## Setup

Nanoka backend is not available for hosting yet.

## Development

Prerequisites:

- [.NET Core SDK](https://www.microsoft.com/net/learn/get-started) 2.2+
- [ElasticSearch](https://www.elastic.co/downloads/elasticsearch) 7+ running locally at `http://localhost:9200`. You can also connect a remote instance by configuring the environment variable `Elastic:Endpoint={{ENDPOINT}}`.
- IDE to ease editing and debugging the source code. [Visual Studio](https://visualstudio.microsoft.com/vs/), [Visual Studio Code](https://code.visualstudio.com/) or [Jetbrains Rider](https://www.jetbrains.com/rider/) is recommended.

## Testing

`Nanoka.Tests` project contains all unit tests written using NUnit.

It is strongly recommend to have a locally running ElasticSearch instance to speed up the testing time. Each unit test method initializes a new set of indexes with a random prefix to avoid collision with other tests. At the end of each test session, **all indexes will be deleted**.
