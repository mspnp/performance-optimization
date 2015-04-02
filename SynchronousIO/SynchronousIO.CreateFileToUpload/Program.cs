// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Text;

namespace SynchronousIO.CreateFileToUpload
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var fileStream = File.Create("FileToUpload.txt"))
            {
                const string loremIpsum =
                    "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nulla vestibulum " +
                    "vitae nisl nec lobortis. Maecenas sit amet dui sed erat pharetra viverra eget et erat. Aliquam " +
                    "ultrices tellus augue, id blandit dolor vestibulum id. Nullam convallis lorem eget neque fermentum " +
                    "tincidunt. Nulla condimentum at elit vel gravida. Quisque pretium neque non massa bibendum dapibus. " +
                    "Nunc sit amet posuere leo. Fusce nec magna et mauris maximus laoreet. Cras bibendum ante neque, nec " +
                    "accumsan est dapibus non. In porta viverra nibh elementum aliquam. Cras semper nibh eget neque " +
                    "placerat, at fermentum eros vulputate. Sed vel orci tincidunt, iaculis nunc at, molestie nunc. " +
                    "Morbi risus lorem, rutrum id rhoncus elementum, interdum quis purus. Vestibulum a magna et massa " +
                    "eleifend vehicula a et nibh. Phasellus varius orci massa, non consectetur est efficitur quis. " +
                    "Aenean posuere ante id neque dapibus fringilla. Aenean sit amet metus semper, interdum ipsum a, " +
                    "aliquet ipsum. In augue purus, tempor id est a volutpat.";

                var buffer = Encoding.ASCII.GetBytes(loremIpsum);

                const int numberOfBuffers = 10240; // Creates a 10 MB file

                for (int i = 0; i < numberOfBuffers; i++)
                {
                    // While file streams do have a WriteAsync method, we're not doing anything in the background.
                    // We don't need to service other threads, nor do we have an async context.
                    // Therefore, the synchronous Write method is fine.

                    fileStream.Write(buffer, 0, buffer.Length);
                }
            }
        }
    }
}
