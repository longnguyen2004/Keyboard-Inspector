﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Keyboard_Inspector {
    class Result: IBinary {
        static readonly char[] Header = new char[] { 'K', 'B', 'I', '\0' };
        static readonly uint FileVersion = 0;
        
        public string GetTitle()
            => string.IsNullOrWhiteSpace(Title)? $"Untitled {Recorded:MM/dd/yyyy, h:mm:ss tt}" : Title;

        public string Title;
        public DateTime Recorded;

        public double Time;
        public List<Event> Events;

        public Result(string title, DateTime recorded, double time, List<Event> events) {
            Title = title?? "";
            Recorded = recorded;
            Time = time;
            
            // Filter auto-repeat
            // TODO Optimize
            Events = events
                .Where((x, i) => !x.Pressed || !(events.Take(i).Where(j => j.Input == x.Input).LastOrDefault()?.Pressed ?? false))
                .ToList();
        }

        public void ToBinary(BinaryWriter bw) {
            bw.Write(Header);
            bw.Write(FileVersion);

            bw.Write(Title);
            bw.Write(Recorded.ToBinary());

            bw.Write(Time);
            Events.ToBinary(bw);
        }

        public static Result FromBinary(BinaryReader br) {
            if (!br.ReadChars(Header.Length).SequenceEqual(Header))
                throw new Exception("Invalid header");

            uint fileVersion = br.ReadUInt32();

            return new Result(
                br.ReadString(),
                DateTime.FromBinary(br.ReadInt64()),
                br.ReadDouble(),
                Event.ListFromBinary(br, fileVersion)
            );
        }

        public static bool IsEmpty(Result result) => (result?.Events.Count?? 0) <= 1;
    }
}
