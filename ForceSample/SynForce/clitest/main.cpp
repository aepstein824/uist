#include <SynKit.h>

//#include <winsock2.h>
#include <windows.h>
#include <fcntl.h>
#include <string.h>
#include <stdlib.h>
#include <errno.h>
#include <stdio.h>
#include <string>
#include <sstream>

#using "System.dll"

using namespace System::Net;
using namespace System::Net::Sockets;

using namespace std;

#pragma comment(lib, "SynCOM.lib") // For access point SynCreateAPI

class SensorPacket {
public:
	double corners[4];
	bool fingerPresent[5];
	double fingersX[5];
	double fingersY[5];
	double fingersZ[5];

	string JSONRep() {
		// Write in JSON as {corners:[c0,c1,c2,c3] , 0:[x,y,z], ..., 4:[x,y,z]}
		stringstream stringbuf = stringstream();
		stringbuf.precision(10);

		//Get corner forces
		bool first = true;
		stringbuf << "{\"corners\":[";
		for (int i = 0; i < 4; ++i) {
			if (!first) stringbuf << ',';
			first = false;
			stringbuf << corners[i];
		}
		stringbuf << "],";

		stringbuf << "\"touched\":[";
		first = true;
		for (int i = 0; i <5; ++i) {
			if (!first) stringbuf << ',';
			first = false;
			stringbuf << fingerPresent[i];
		}
		stringbuf << "],";

		first = true;
		//write in each finger
		for (int i = 0; i < 5; ++i) {
			if (!first) stringbuf << ',';
			first = false;
			stringbuf << '\"' << 'f' << i << "\":[";
			stringbuf << fingersX[i] << ',';
			stringbuf << fingersY[i] << ',';
			stringbuf << fingersZ[i] << ']';
		}
		stringbuf << '}' << endl;

		return stringbuf.str();
	}
};

SensorPacket lastPacket;
int lastPacketIndex;

DWORD WINAPI SocketHandler(void*);
DWORD WINAPI SensorLoop(void *argPointer);

System::String^ getIPAddress()
{
    System::String^ localIP = "?";
    IPHostEntry ^host = Dns::GetHostEntry(Dns::GetHostName());
    for each (IPAddress^ ip in host->AddressList)
    {
        if (ip->AddressFamily == AddressFamily::InterNetwork)
        {
            localIP = ip->ToString();
        }
    }
    return localIP;
}

int main(int argv, char** argc){
	/*
	//System::String^ ipAddress = (gcnew WebClient())->DownloadString("http://myip.ozymo.com/");
	System::String^ ipAddress = getIPAddress();
	System::Console::WriteLine("your ip address is: " + ipAddress);
    (gcnew WebClient())->DownloadString("http://transgame.csail.mit.edu:9537/?set=" + ipAddress + "&varname=jedeyeserver");
    */
	//The port you want the server to listen on
    int host_port= 1101;

    //Initialize socket support WINDOWS ONLY!
    unsigned short wVersionRequested;
    WSADATA wsaData;
    int err;
    wVersionRequested = MAKEWORD( 2, 2 );
    err = WSAStartup( wVersionRequested, &wsaData );
    if ( err != 0 || ( LOBYTE( wsaData.wVersion ) != 2 ||
            HIBYTE( wsaData.wVersion ) != 2 )) {
        fprintf(stderr, "Could not find useable sock dll %d\n",WSAGetLastError());
        exit (1);
    }

    //Initialize sockets and set any options
    int hsock;
    int * p_int ;
    hsock = socket(AF_INET, SOCK_STREAM, 0);
    if(hsock == -1){
        printf("Error initializing socket %d\n",WSAGetLastError());
        exit (1);
    }
    
    p_int = (int*)malloc(sizeof(int));
    *p_int = 1;
    if( (setsockopt(hsock, SOL_SOCKET, SO_REUSEADDR, (char*)p_int, sizeof(int)) == -1 )||
        (setsockopt(hsock, SOL_SOCKET, SO_KEEPALIVE, (char*)p_int, sizeof(int)) == -1 ) ){
        printf("Error setting options %d\n", WSAGetLastError());
        free(p_int);
        exit (1);
    }
    free(p_int);

    //Bind and listen
    struct sockaddr_in my_addr;

    my_addr.sin_family = AF_INET ;
    my_addr.sin_port = htons(host_port);
    
    memset(&(my_addr.sin_zero), 0, 8);
    my_addr.sin_addr.s_addr = INADDR_ANY ;
    
    if( bind( hsock, (struct sockaddr*)&my_addr, sizeof(my_addr)) == -1 ){
        fprintf(stderr,"Error binding to socket, make sure nothing else is listening on this port %d\n",WSAGetLastError());
        exit (1);
    }
    if(listen( hsock, 10) == -1 ){
        fprintf(stderr, "Error listening %d\n",WSAGetLastError());
        exit (1);
    }
    
	lastPacketIndex = 0;
	for (int i = 0; i < 5; i++)
	{
		lastPacket.corners[i < 4 ? i : 0] = 0.;
		lastPacket.fingerPresent[i] = false;
		lastPacket.fingersX[i] = 0.;
		lastPacket.fingersY[i] = 0.;
		lastPacket.fingersZ[i] = 0.;
	}

	//create the thread that does the sensor stuff
	CreateThread(0,0,&SensorLoop, NULL , 0,0);

    //Now lets to the server stuff
    int* csock;
    sockaddr_in sadr;
    int    addr_size = sizeof(SOCKADDR);
    
    while(true){
        printf("waiting for a connection\n");
        csock = (int*)malloc(sizeof(int));
        
        if((*csock = accept( hsock, (SOCKADDR*)&sadr, &addr_size))!= INVALID_SOCKET ){
            printf("Received connection from %s",inet_ntoa(sadr.sin_addr));
            CreateThread(0,0,&SocketHandler, (void*)csock , 0,0);
        }
        else{
            fprintf(stderr, "Error accepting %d\n",WSAGetLastError());
        }
    }
}

DWORD WINAPI SocketHandler(void* lp){
    int *csock = (int*)lp;

	int lastSentIndex = -1;

    char buffer[1024];
    int buffer_len = 1024;
    int bytecount;

	/*
	//no need to receive
    if((bytecount = recv(*csock, buffer, buffer_len, 0))==SOCKET_ERROR){
        fprintf(stderr, "Error receiving data %d\n", WSAGetLastError());
        goto FINISH;
    }
    printf("Received bytes %d\nReceived string \"%s\"\n", bytecount, buffer);
	*/
	while (true)
	{
		if (lastSentIndex < lastPacketIndex)
		{
			// Send sensor data
			memset(buffer, 0, buffer_len);
			string s = lastPacket.JSONRep();
			strcat_s (buffer, s.size() + 1, s.c_str());

			if((bytecount = send(*csock, buffer, strlen(buffer), 0))==SOCKET_ERROR)
			{
				fprintf(stderr, "Error sending data %d\n", WSAGetLastError());
				break;
			}
		}
		Sleep (17); //1/60
	}
    
    printf("Sent bytes %d\n", bytecount);
    free(csock);
    return 0;
}

DWORD WINAPI SensorLoop(void *argPointer)
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
			for (int i = 0; i < 4; i++)
			{ 
				lastPacket.corners[i] = lForceRaw[i];
			}

			for (int i = 0; i < 5; i++)
			{
				lastPacket.fingerPresent [i] = false;
			}

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
					lastPacket.fingerPresent [i] = true;
                    ++lFingerCount;
                    // Extract the position and force of the touch
                    LONG lX, lY, lZForce;
                    pPacket->GetProperty(SP_X, &lX);
                    pPacket->GetProperty(SP_Y, &lY);
                    pPacket->GetProperty(SP_ZForce, &lZForce);
                    printf("    Touch %d: Coordinates (%4d, %4d), force +%3d grams\n", i, lX, lY, lZForce);
					lastPacket.fingersX[i] = lX;
					lastPacket.fingersY[i] = lY;
					lastPacket.fingersZ[i] = lZForce;
                }
            }
			string str = lastPacket.JSONRep();
			printf(str.c_str());
			lastPacketIndex++;
        }
    }
    while (true);

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
