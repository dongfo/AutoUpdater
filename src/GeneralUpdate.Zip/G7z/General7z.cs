﻿using GeneralUpdate.Zip.Events;
using GeneralUpdate.Zip.Factory;
using SharpCompress.Archives;
using SharpCompress.Common;
using SharpCompress.IO;
using SharpCompress.Readers;
using SharpCompress.Writers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GeneralUpdate.Zip.G7z
{
    /// <summary>
    /// Source address : https://github.com/adamhathcock/sharpcompress/blob/master/tests/SharpCompress.Test/WriterTests.cs
    /// Author : Adam hathcock .
    /// Tribute to the original author .
    /// Secondary developer : Juster Z
    /// </summary>
    public class General7z : BaseCompress, IOperation
    {
        private string _sourcePath, _destinationPath;
        private Encoding _encoding;
        private int _unZipCount = 0;
        private int _unzipTotalCount = 0;

        public delegate void UnZipProgressEventHandler(object sender, BaseUnZipProgressEventArgs e);
        public event UnZipProgressEventHandler UnZipProgress;

        public delegate void CompressProgressEventHandler(object sender, BaseCompressProgressEventArgs e);
        public event CompressProgressEventHandler CompressProgress;

        public bool CreatZip()
        {
            try
            {
                using (Stream stream = File.OpenWrite(_sourcePath))
                {
                    WriterOptions writerOptions = new WriterOptions(CompressionType.Unknown)  { LeaveStreamOpen = true  };
                    writerOptions.ArchiveEncoding.Default = _encoding;
                    using (var writer = WriterFactory.Open(stream, ArchiveType.SevenZip, writerOptions))
                    {
                        writer.WriteAll(SOLUTION_BASE_PATH, "*", SearchOption.AllDirectories);
                    }
                }
                using (Stream stream = File.OpenRead(_destinationPath))
                {
                    ReaderOptions readerOptions = new ReaderOptions();
                    readerOptions.ArchiveEncoding.Default = _encoding;
                    using (var reader = ReaderFactory.Open(new NonDisposingStream(stream), readerOptions))
                    {
                        reader.WriteAllToDirectory(SOLUTION_BASE_PATH, new ExtractionOptions() { ExtractFullPath = true });
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool UnZip()
        {
            try
            {
                bool isComplete = false;
                using (var archive = ArchiveFactory.Open(SOURSE_PATH, null))
                {
                   var entries =  archive.Entries;
                    _unzipTotalCount = entries.Count();
                    archive.FilePartExtractionBegin += OnFilePartExtractionBegin;
                    foreach (var entry in entries.Where(entry => !entry.IsDirectory))
                    {
                        _unZipCount++;
                        entry.WriteToDirectory(_destinationPath, new ExtractionOptions() {  ExtractFullPath = true,  Overwrite = true });
                    }
                    isComplete = archive.IsComplete;
                }
                return isComplete;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void OnFilePartExtractionBegin(object sender, FilePartExtractionBeginEventArgs e)
        {
            if (UnZipProgress != null) UnZipProgress(sender,new BaseUnZipProgressEventArgs() { Size = e.CompressedSize, Name = e.Name , Index = _unZipCount , Count = _unzipTotalCount , Path = Path.Combine(_destinationPath,e.Name) });
        }

        public void OnCompressProgressEventHandler(object sender, BaseCompressProgressEventArgs e)
        {
            if (CompressProgress != null) CompressProgress(sender, e);
        }

        public void OnUnZipProgressEventHandler(object sender, BaseUnZipProgressEventArgs e)
        {
            if (UnZipProgress != null) UnZipProgress(sender, e);
        }

        public void Configs(string sourcePath, string destinationPath = null)
        {
            _sourcePath = sourcePath;
            _destinationPath = destinationPath ?? SOLUTION_BASE_PATH;
            _encoding = _encoding ?? Encoding.UTF8;
            Verifypath(sourcePath, destinationPath);
        }

        public void Configs(string sourcePath, string destinationPath, Encoding encoding)
        {
            _encoding = encoding ?? Encoding.UTF8;
            Configs(sourcePath, destinationPath);
        }
    }
}