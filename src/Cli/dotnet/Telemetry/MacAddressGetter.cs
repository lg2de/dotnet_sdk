// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using System.ComponentModel;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using Microsoft.DotNet.Cli.Utils.Extensions;

namespace Microsoft.DotNet.Cli.Telemetry;

internal static class MacAddressGetter
{
    private const string MacRegex = @"(?:[a-z0-9]{2}[:\-]){5}[a-z0-9]{2}";
    private const string ZeroRegex = @"(?:00[:\-]){5}00";
    private const int ErrorFileNotFound = 0x2;
    public static string GetMacAddress()
    {
        try
        {
            var shelloutput = GetShellOutMacAddressOutput();
            if (shelloutput == null)
            {
                return null;
            }

            return ParseMACAddress(shelloutput);
        }
        catch (Win32Exception e)
        {
            if (e.NativeErrorCode == ErrorFileNotFound)
            {
                return GetMacAddressByNetworkInterface();
            }
            else
            {
                throw;
            }
        }
    }

    private static string ParseMACAddress(string shelloutput)
    {
        string macAddress = null;
        foreach (Match match in Regex.Matches(shelloutput, MacRegex, RegexOptions.IgnoreCase))
        {
            if (!Regex.IsMatch(match.Value, ZeroRegex))
            {
                macAddress = match.Value;
                break;
            }
        }

        if (macAddress != null)
        {
            return macAddress;
        }
        return null;
    }

    private static string GetIpCommandOutput()
    {
        var fileName = File.Exists(@"/usr/bin/ip") ? @"/usr/bin/ip" :
                       File.Exists(@"/usr/sbin/ip") ? @"/usr/sbin/ip" :
                       File.Exists(@"/sbin/ip") ? @"/sbin/ip" :
                       "ip";
        var ipResult = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = "link",
            UseShellExecute = false
        }.ExecuteAndCaptureOutput(out string ipStdOut, out _);

        if (ipResult == 0)
        {
            return ipStdOut;
        }
        else
        {
            return null;
        }
    }

    private static string GetShellOutMacAddressOutput()
    {
        if (OperatingSystem.IsWindows())
        {
            var result = new ProcessStartInfo
            {
                FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "getmac.exe"),
                UseShellExecute = false
            }.ExecuteAndCaptureOutput(out string stdOut, out _);

            if (result == 0)
            {
                return stdOut;
            }
            else
            {
                return null;
            }
        }
        else
        {
            try
            {
                var fileName = File.Exists("/sbin/ifconfig") ? "/sbin/ifconfig" :
                               File.Exists("/usr/sbin/ifconfig") ? "/usr/sbin/ifconfig" :
                               File.Exists("/usr/bin/ifconfig") ? "/usr/bin/ifconfig" :
                               "ifconfig";

                var ifconfigResult = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = "-a",
                    UseShellExecute = false
                }.ExecuteAndCaptureOutput(out string ifconfigStdOut, out string ifconfigStdErr);

                if (ifconfigResult == 0)
                {
                    return ifconfigStdOut;
                }
                else
                {
                    return GetIpCommandOutput();
                }
            }
            catch (Win32Exception e)
            {
                if (e.NativeErrorCode == ErrorFileNotFound)
                {
                    return GetIpCommandOutput();
                }
                else
                {
                    throw;
                }
            }
        }
    }

    private static string GetMacAddressByNetworkInterface()
    {
        return GetMacAddressesByNetworkInterface().FirstOrDefault();
    }

    private static List<string> GetMacAddressesByNetworkInterface()
    {
        NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
        var macs = new List<string>();

        if (nics == null || nics.Length < 1)
        {
            macs.Add(string.Empty);
            return macs;
        }

        foreach (NetworkInterface adapter in nics)
        {
            IPInterfaceProperties properties = adapter.GetIPProperties();

            PhysicalAddress address = adapter.GetPhysicalAddress();
            byte[] bytes = address.GetAddressBytes();
            macs.Add(string.Join("-", bytes.Select(x => x.ToString("X2"))));
            if (macs.Count >= 10)
            {
                break;
            }
        }
        return macs;
    }
}
