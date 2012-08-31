#include <SynKit.h>

#pragma comment(lib, "SynCOM.lib") // For access point SynCreateAPI

int main(int argc, char** argv)
{
    // Wait object will indicate when new data is available
    HANDLE hEvent = CreateEvent(NULL, FALSE, FALSE, NULL);
    
    // Entry point to Synaptics API
    ISynAPI* pAPI = NULL;
    SynCreateAPI(&pAPI);
    
    // Find the first USB TouchPad device connected to the system
    LONG lHandle = -1;
    if (pAPI->FindDevice(SE_ConnectionUSB, SE_DeviceTouchPad, &lHandle) == SYNE_NOTFOUND)
    {
        printf("ForcePad not found\n");
        return EXIT_FAILURE;
    }
    
    // Create an interface to the ForcePad
    ISynDevice* pDevice = NULL;
    pAPI->CreateDevice(lHandle, &pDevice);

    // Tell the device to signal hEvent when data is ready
    pDevice->SetEventNotification(hEvent);

    // Enable multi-finger touch and grouped reporting
    pDevice->SetProperty(SP_IsMultiFingerReportEnabled, 1);
    pDevice->SetProperty(SP_IsGroupReportEnabled, 1);

    // Get the maximum number of fingers the device will report
    LONG lNumMaxReportedFingers;
    pDevice->GetProperty(SP_NumMaxReportedFingers, &lNumMaxReportedFingers);

    // Create an ISynGroup instance to receive per-frame data
    ISynGroup* pGroup = NULL;
    pDevice->CreateGroup(&pGroup);
    
    // Create an ISynPacket instance to receive per-touch data
    ISynPacket* pPacket;
    pDevice->CreatePacket(&pPacket);

    // Stop the ForcePad reporting to the operating system
    pDevice->Acquire(SF_AcquireAll);


    printf("Touch the surface to see properties\n");
    printf("Touch with %d fingers to quit\n", lNumMaxReportedFingers);

    LONG lFingerCount = 0;
    do
    {
        // Wait until the event signals that data is ready
        WaitForSingleObject(hEvent, INFINITE);

        // Load data into the ISynGroup instance, repeating until there is no more data
        while (pDevice->LoadGroup(pGroup) != SYNE_FAIL)
        {
            // Extract the raw values of the 4 corner force sensors
            LONG lForceRaw[4];
            for (LONG i = 0; i != 4; ++i)
                pGroup->GetPropertyByIndex(SP_ForceRaw, i, lForceRaw + i);
            printf("Corner forces (grams) [%+3d, %+3d, %+3d, %+3d]\n", lForceRaw[0], lForceRaw[1], lForceRaw[2], lForceRaw[3]);
            
            // For each touch (packet)
            lFingerCount = 0;
            for (LONG i = 0; i != lNumMaxReportedFingers; ++i)
            {
                // Load data into the SynPacket object
                pGroup->GetPacketByIndex(i, pPacket);
                // Is there a finger present?
                LONG lFingerState;
                pPacket->GetProperty(SP_FingerState, &lFingerState);
                if (lFingerState & SF_FingerPresent)
                {
                    ++lFingerCount;
                    // Extract the position and force of the touch
                    LONG lX, lY, lZForce;
                    pPacket->GetProperty(SP_X, &lX);
                    pPacket->GetProperty(SP_Y, &lY);
                    pPacket->GetProperty(SP_ZForce, &lZForce);
                    printf("    Touch %d: Coordinates (%4d, %4d), force +%3d grams\n", i, lX, lY, lZForce);
                }
            }
        }
    }
    while (lFingerCount < lNumMaxReportedFingers);

    printf("%d finger gesture detected; exiting\n", lNumMaxReportedFingers);

    // Don't signal any more data
    pDevice->SetEventNotification(NULL);
    
    // Release the COM objects we have created
    pPacket->Release();
    pGroup->Release();
    pDevice->Release();
    pAPI->Release();

    // Release the wait object
    CloseHandle(hEvent);

    return EXIT_SUCCESS;
    
}
