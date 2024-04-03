﻿using Horiba.Sdk.Communication;

namespace Horiba.Sdk.Devices;

public record MonochromatorDevice(int DeviceId, string DeviceType, string SerialNumber, WebSocketCommunicator Communicator) : Device(DeviceId, DeviceType, SerialNumber, Communicator)
{
    public override Task<bool> IsConnectionOpenedAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task OpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task CloseConnectionAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}