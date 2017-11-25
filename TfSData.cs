
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;




namespace TFSStatistics
{
    class TfSData
    {

        class SourceElement
        {
            public string filename;
            public int numberOfModification;
            public long contentLenght;
            public DateTime startDate;
            public int lineCount;

        }
        public void doResearch(System.String userNmae, System.String pass)
        {


            Uri adminUiri = new Uri("xxxxxxxxxxxxxxxx");
            TfsTeamProjectCollection projectCollection = new
            TfsTeamProjectCollection(adminUiri, new System.Net.NetworkCredential("xxx", "xxxxx"));


            projectCollection.EnsureAuthenticated();
            Workspace workspace = null;
            Boolean createdWorkspace = false;
            String newFolder = String.Empty;

            VersionControlServer versionControl = projectCollection.GetService<VersionControlServer>();

            var teamProjects = new List<TeamProject>(versionControl.GetAllTeamProjects(false));



            String workspaceName = String.Format("{0}-{1}", Environment.MachineName, "Test");
            try
            {
                workspace = versionControl.GetWorkspace(workspaceName, versionControl.AuthorizedUser);
            }
            catch (WorkspaceNotFoundException)
            {
                workspace = versionControl.CreateWorkspace(workspaceName, versionControl.AuthorizedUser);
                createdWorkspace = true;
            }
            //int id = workspace.VersionControlServer.GetLatestChangesetId();

            var serverFolder = String.Format("$/{0}", teamProjects[0].Name) + "/xxxxx";
            var localFolder = "D:/xxxxxxx/";
            var workingFolder = new WorkingFolder(serverFolder, localFolder);

            // Create a workspace mapping.
            workspace.CreateMapping(workingFolder);

            if (!workspace.HasReadPermission)
            {
                throw new SecurityException(
                String.Format("{0} does not have read permission for {1}",
                versionControl.AuthorizedUser, serverFolder));
            }

            // Get the files from the repository.
            workspace.Get();
            string[] directories = Directory.GetDirectories(workspace.Folders[0].LocalItem);
   
            System.Collections.Generic.List<SourceElement> fileLiset =
                new System.Collections.Generic.List<SourceElement>();

            foreach (string dir in directories)
            {
                foreach (string file in Directory.GetFiles(dir))
                {

                    string filenamae = System.IO.Path.GetFileName(file);
                    var lineCount = File.ReadLines(dir + "\\"+ filenamae).Count();
                    Item source = versionControl.GetItem(file);
                    System.Collections.IEnumerable history = versionControl.QueryHistory(file,
                    VersionSpec.Latest, 0, RecursionType.Full, null, null, null, 300, true, true, false, false);
                    int numberOfModification = 0;
                    foreach (Changeset item in history)
                    {

                        SourceElement fileElement = new SourceElement();
                        fileElement.filename = filenamae;
                        fileElement.numberOfModification = numberOfModification;
                        fileElement.contentLenght = source.ContentLength;
                        fileElement.lineCount = lineCount;
                        fileElement.startDate = item.CreationDate;
                        fileLiset.Add(fileElement);
                    }
                }
            }
            var sortedList = fileLiset.OrderBy(x => x.numberOfModification);
            // Loop through keys.
            FileStream outputFile = new FileStream("TfsQueryResults.txt", FileMode.Create);
            StreamWriter writer = new StreamWriter(outputFile);
            foreach (var key in sortedList)
            {
                writer.WriteLine("{0}; {1} ;{2}",
                    key.filename, 
                    key.lineCount,
                    key.startDate
                    );
            }
            writer.Close();
        }


    }
}
