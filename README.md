![Icon](https://i.imgur.com/5ps5Ewf.png)
# SimInformation

SimInformation is a cross-platform library that provides a way to access the following information from your SIM card:

- Integrated Circuit Card ID. (ICCID)
- Mobile Country Code (MCC)
- International Mobile Subscriber Identity. (IMSI)
- Mobile Station ID (MSID)
- Mobile Network Code. (MNC)
- Mobile Subscriber International ISDN Number. (MSISDN)

# Supported Platforms

## Universal Windows Platform

The API's to retrieve SIM information are restricted by Microsoft:

> Special and restricted capabilities are intended for very specific scenarios. 
> The use of these capabilities is highly restricted and subject to additional Store onboarding policy and review.
> https://msdn.microsoft.com/en-us/library/windows/apps/mt270968.aspx

The following restrictions apply:

> Apps that apply the special-use capabilities require a company account to submit them to the Store. 
> Restricted capabilities are available only to apps that are developed by Microsoft and its partners. 
> All restricted capabilities must include the rescap namespace when you declare them in your app's package manifest differently than other capabilities. 

With that all said and done, manually edit your application `Package.appxmanifest` as follows:

* Add the XML namespace as follows:

```cs
xmlns:rescap="http://schemas.microsoft.com/appx/manifest/foundation/windows10/restrictedcapabilities" 
```

* Add the following capability which will allow the library access to this information.
	
```cs
<Capabilities>
    <rescap:Capability Name="cellularDeviceIdentity" />
</Capabilities>
```

# With thanks to
* The icon "<a href="https://thenounproject.com/term/sim-card/15159">SIM Card</a>" designed by <a href="https://thenounproject.com/misirlou">misirlou</a> from The Noun Project.