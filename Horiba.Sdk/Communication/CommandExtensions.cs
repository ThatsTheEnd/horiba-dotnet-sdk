﻿using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Horiba.Sdk.Communication;

internal static class CommandExtensions
{
    public static byte[] ToByteArray(this Command command)
    {
        return Encoding.UTF8.GetBytes(SerializeCommand(command));
    }
    
    public static string ToJson(this Command command)
    {
        return SerializeCommand(command);
    }

    private static string SerializeCommand(Command command)
    {
        return JsonConvert.SerializeObject(command, new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        });
    }
}