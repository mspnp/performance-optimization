using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynchronousIO
{
    public static class CreateFile
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

        const int DefaultNumberOfCopies = 10240; // Creates a ~10 MB file

        public static Stream Get(int numberOfCopies = DefaultNumberOfCopies)
        {
            var memoryStream = new MemoryStream();
            var writer = new StreamWriter(memoryStream, Encoding.UTF8);

            for (int i = 0; i < numberOfCopies; i++)
            {
                // While file streams do have a WriteAsync method, we're not doing anything in the background.
                // We don't need to service other threads, nor do we have an async context.
                // Therefore, the synchronous Write method is fine.
                writer.Write(loremIpsum);
            }
            memoryStream.Position = 0;
            return memoryStream;
        }
    }
}
