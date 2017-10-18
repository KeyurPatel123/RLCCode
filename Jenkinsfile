try {
	node('Windows') {
		stage 'Checkout'
			checkout scm

		stage 'Build'
			
			bat '''SET BRANCHname=%GIT_BRANCH:origin/=%

				nuget restore .

				dotnet publish .\\Abiomed.DotNetCore.FactoryData.Loader\\Abiomed.DotNetCore.FactoryData.Loader.csproj -c Release /p:PublishDir="..\\FactoryData"
				7z a FactoryData_v01_%BRANCHname%_%BUILD_NUMBER%.zip FactoryData/

				dotnet publish .\\Abiomed.DotNetCore.MailQueueService\\Abiomed.DotNetCore.MailQueueService.csproj -c Release /p:PublishDir="..\\MailQueueService"
				7z a MailQueueService_v01_%BRANCHname%_%BUILD_NUMBER%.zip MailQueueService/

				dotnet publish .\\Abiomed.DotNetCore.MessagePump\\Abiomed.DotNetCore.MessagePump.csproj -c Release /p:PublishDir="..\\MessagePump"
				7z a MessagePump_v01_%BRANCHname%_%BUILD_NUMBER%.zip MessagePump/

				dotnet publish .\\Abiomed.DotNetCore.OCRService\\Abiomed.DotNetCore.OCRService.csproj -c Release /p:PublishDir="..\\OCRService"
				7z a OCRService_v01_%BRANCHname%_%BUILD_NUMBER%.zip OCRService/

				dotnet publish .\\Abiomed.RLR\\Abiomed.RLR.csproj -c Release /p:PublishDir="..\\RLR"
				7z a RLR_v01_%BRANCHname%_%BUILD_NUMBER%.zip RLR/
			'''

		stage 'Archive'
			archive '*.zip'

	}
	mail ( 
        to: 'ppamidi@abiomed.com',
        subject: "RemoteLinkCloud Multi-Branch Pipeline -  Build Notification - SUCCESS - New Code Pushed to Git", 
        body: """RemoteLinkCloud Multi-Branch Pipeline - SUCCESS"""
    
    )
} catch(e) {
    echo e.message
    mail( to: 'ppamidi@abiomed.com',
          subject: 'RemoteLinkCloud Multi-Branch Pipeline -  Build Notification - FAILED - New Code Pushed to Git', 
          body: "See buildlog at ${env.BUILD_URL}/console")
    error e.message
}
