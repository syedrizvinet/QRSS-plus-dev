﻿using Azure.Storage.Blobs;
using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QrssPlus.FileStorage
{
    public static class FileAction
    {
        public static void UpdateFiles(GrabberList grabbers, int maxAgeMinutes, string storageConnectionString)
        {
            BlobContainerClient container = new(storageConnectionString, "$web");

            Grabber[] activeGrabbers = grabbers.Select(x => x).Where(x => x.Grab.Data != null).ToArray();

            Parallel.ForEach(activeGrabbers, grabber =>
            {
                string filename = grabber.GetFileName();
                BlobClient blob = container.GetBlobClient(filename);
                using var stream = new MemoryStream(grabber.Grab.Data, writable: false);
                blob.Upload(stream);
                stream.Close();
            });

            foreach (var item in container.GetBlobs())
            {
                var age = DateTime.UtcNow - item.Properties.LastModified;
                if (age > TimeSpan.FromMinutes(maxAgeMinutes))
                    container.DeleteBlob(item.Name);
            }

        }
    }
}
