# cuber - reverse TCP and UDP proxy server

## Content

* [About the project](#about-the-project)
* [Features](#features)
* [ToDo](#todo)
* [Usage](#usage)
* [Config](#config)
* [Contributing](#contributing)
* [License](#license)

## About the project

Cuber is a reverse proxy server with support for TCP and UDP traffic and the ability to load balance between different target (backend) servers.
Health checks are also available to dynamically remove backend servers from the load balance strategy, so that traffic is not forwarded to offline or crashed servers.

## Features

* wide variety of load balance strategies
  * round-robin
  * random
  * least connection
  * hash/sticky (based on source ip, uses the same backend server for multiple requests)
* proxying common transport protocols
  * TCP
  * UDP
* simultaneous TCP and UDP proxying on same port
* health checks to minimize service interruption if backend servers fail
  * TCP (checks if a connection can be established)
  * HTTP (requests a configurable path and checks for success status code)

## ToDo

* [ ] Load balance strategies
  * [X] round robin
  * [X] random
  * [ ] weighted round robin
  * [X] least connection
  * [X] hash/sticky
* [X] TCP proxying
* [X] UDP proxying
* [X] TCP and UDP proxying combined
* [X] health checks
  * [X] TCP
  * [X] HTTP
* [ ] both internet protocol versions
  * [X] IPv4
  * [ ] IPv6
* [ ] custom read and write timeout handling
* [ ] highly configurable

## Usage

TODO

## Config

The config is a json file named 'cuber.json' in the same directory the executable is.

Descriptions and default values for the 'Cuber' section can be found below:

| Key | Default  | Description  |
|:---|:---:|---|
| Ip | "127.0.0.1"  | The IP address where cuber binds to. |
| Port | 50000 | The port where cuber binds to.  |
| Mode | "tcp" | In which mode should cuber run ('tcp', 'udp' and 'multi').  |
| BalanceStrategy | "RoundRobin" | Load balancing strategy between backend servers to use. Supported are "RoundRobin", "Random", "LeastConnection" and "Hash". |
| Targets | [] | A json array with objects defining the backend servers with IP and port. |
| UpStreamBufferSize | 8192 | Upstream buffer size when receiving data from clients. |
| DownStreamBufferSize | 8192 | Downstream buffer size when receiving data from backend servers. |
| Urls | ["http://localhost:50001"] | A string json array specifying the urls the REST API will be listening on |
| ApiKeyHeaderName | "X-Api-Key" | The request header name in which the api key is. |
| ApiKey | "changeme" | The API key to authenticate request against the REST API. |
| Type | "tcp" | The health probe protocol to use ('tcp' or 'http'). |
| Port | port of the backend server | The port to use for health probe requests. |
| Path | "/" | The path to request when using 'http' as health probe. |
| Timeout | 5000 | The connect/request timeout in milliseconds. |
| Interval | 5000 | The interval in milliseconds at which the health probes are executed. |

Example config file with all possible values:

```json
{
    "Cuber": {
        "Ip": "127.0.0.1",
        "Port": 50000,
        "Backlog": 25,
        "Mode": "tcp",
        "BalanceStrategy": "RoundRobin",
        "Targets": [
            {
                "ip": "10.0.1.1",
                "port": 31337
            }
        ],
        "UpStreamBufferSize": 8192,
        "DownStreamBufferSize": 8192,
        "Web": {
            "Urls": ["http://localhost:50001"],
            "ApiKeyHeaderName": "X-Api-Key",
            "ApiKey": "changeme"
        },
        "HealthProbe": {
            "Type": "http",
            "Port": 8080,
            "Path": "/health",
            "Timeout": 2000,
            "Interval": 10000
        }
    },
    "Logging": {
        "IncludeScopes": true,
        "LogLevel": {
            "Default": "Debug",
            "System": "Warning",
            "Microsoft": "Warning"
        },
        "Console": {
            "LogLevel": {
                "Default": "Debug",
                "System": "Warning",
                "Microsoft": "Warning"
            }
        }
    }
}
```

## Contributing

Any contibutions are greatly appreciated.
Just fork the project, create a new feature branch, commit and push your changes and open a pull request.

## License

Distributed under the MIT License. See LICENSE for more information.
