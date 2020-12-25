# cuber - reverse TCP and UDP proxy server

## Content

* [About the project](#about-the-project)
* [Features](#features)
* [ToDo](#todo)
* [Usage](#usage)
* [Contributing](#contributing)
* [License](#license)

### About the project

Cuber is a reverse proxy server with support for TCP and UDP traffic and the ability to load balance between different target (backend) servers.
Health checks are also available to dynamically remove backend servers from the load balance strategy, so that traffic is not forwarded to offline or crashed servers.

### Features

Soon.

### ToDo

* [ ] Load balance strategies
  * [X] round robin
  * [X] random
  * [ ] weighted round robin
  * [X] least connection
  * [ ] hash/sticky
* [X] TCP proxying
* [X] UDP proxying
* [ ] TCP and UDP proxying combined
* [X] health checks
  * [X] TCP
  * [X] HTTP
* [ ] both internet protocol versions
  * [X] IPv4
  * [ ] IPv6
* [ ] custom read and write timeout handling
* [ ] highly configurable

### Usage

TODO

### Contributing

Any contibutions are greatly appreciated.
Just fork the project, create a new feature branch, commit and push your changes and open a pull request.

### License

Distributed under the MIT License. See LICENSE for more information.
