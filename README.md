# OxHack.Doorbell
SMS based doorbell system.

## The SMS Daemon
The SmsDaemon sits listening for SMSes and then relays the to a RabbitMQ broker for other appliactions to consume.

### Building it
1. Install `nuget`
2. Go into the `OxHack.Doorbell.SmsDaemon` folder and do a `nuget restore`
3. You should then be able to use `xbuild OxHack.Doorbell.SmsDaemon.csproj` to build it.

### Setting it up
It set up my instance on a standard raspberry pi.  It has the following package dependencies:

* Mono`apt-get install mono-complete`
* RabbitMQ `apt-get install rabbitmq-server`
* Gammu SMS Daemon `apt-get install gammu-smsd`

Once those things are installed you should be able to run `SmsDaemonDeployment/deploy.sh` (use `sudo` or run as root) to set up the basic plumbing.

Note: You'll probably have to go muck in `/etc/gammu-smsdrc` yourself to set the appropriate `port` value for your device.  Leave the `RunOnReceive` value as is.

Next, you need to set up a RabbitMQ account for `OxHack.Doorbell.SmsDaemon.exe` to use.  It needs to have enough rights to create exchanges, queues, and to write to them.

And the final step is to update `OxHack.Doorbell.SmsDaemon.exe.config` to reflect your RabbitMQ configuration.
