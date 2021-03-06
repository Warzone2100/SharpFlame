﻿using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using SharpFlame.Core.Extensions;
using SharpFlame.Core.Parsers.Lev;
using SharpFlame.Core.Parsers.Lev;
using Sprache;

namespace SharpFlame.Tests.Parser
{
    [TestFixture]
    public class LevParseTests
    {
        [Test]
        public void CanParseToken()
        {
            var data = @"level   test_flame-T1
test   test123
";
            var result = LevGrammar.Token.Parse(data);
            result.Name.Should ().Be ("level");
            result.Data.Should().Be("test_flame-T1");
        }

        public void CanParseQuottedIdentifier()
        {
            var data = "data    \"wrf/multi/skirmish2.wrf\"";
            var result = LevGrammar.Token.Parse(data);
            result.Name.Should ().Be ("data");
            result.Data.Should().Be("wrf/multi/skirmish2.wrf");
        }

        [Test]
        public void CanParseCampaign()
        {
            var data = @"campaign	MULTI_CAM_1
data		""wrf/vidmem.wrf""
data		""wrf/basic.wrf""
data		""wrf/cam1.wrf""
data		""wrf/audio.wrf""
data		""wrf/piestats.wrf""
data		""wrf/stats.wrf""
data		""wrf/multires.wrf""

campaign    MULTI_CAM_2
data        ""wrf/vidmem2.wrf""
data        ""wrf/basic.wrf""
data        ""wrf/cam2.wrf""
data        ""wrf/audio.wrf""
data        ""wrf/piestats.wrf""
data        ""wrf/stats.wrf""
data        ""wrf/multires.wrf""";

            var result = LevGrammar.Campaign.Parse (data);
            result.Data.Count.Should ().Be (7);
            result.Data [4].Should ().Be ("wrf/piestats.wrf");
        }

        [Test]
        public void CanParseLevel()
        {
            var data = @"level   Tinny-War-T1
players 2
type    14
dataset MULTI_CAM_1
game    ""multiplay/maps/2c-Tinny-War.gam""
data    ""wrf/multi/skirmish2.wrf""
data    ""wrf/multi/fog1.wrf""

level   Tinny-War-T2
players 2
type    18
dataset MULTI_T2_C1
game    ""multiplay/maps/2c-Tinny-War.gam""
data    ""wrf/multi/t2-skirmish2.wrf""
data    ""wrf/multi/fog1.wrf""

level   Tinny-War-T3
players 2
type    19
dataset MULTI_T3_C1
game    ""multiplay/maps/2c-Tinny-War.gam""
data    ""wrf/multi/t3-skirmish2.wrf""
data    ""wrf/multi/fog1.wrf""";

            var result = LevGrammar.Level.Parse (data);
            result.Name.Should ().Be ("Tinny-War-T1");
            result.Players.Should ().Be (2);
            result.Type.Should ().Be (14);
            result.Dataset.Should ().Be ("MULTI_CAM_1");
            result.Game.Should().Be ("multiplay/maps/2c-Tinny-War.gam");
            result.Data.Count.Should ().Be (2);
            result.Data [0].Should ().Be ("wrf/multi/skirmish2.wrf");
            result.Data [1].Should ().Be ("wrf/multi/fog1.wrf");
        }

        [Test]
        public void CanParseAddonLev()
        {
            var file = Path.Combine ("Data", 
                                     Path.Combine ("Levels", 
                                     Path.Combine ("addon.lev")));

            var txt = File.ReadAllText( file );
            Console.WriteLine( "Parsing: {0}", file );
            var levfile = LevGrammar.Lev.Parse (txt);

            Console.WriteLine ("# of campaigns: {0}", levfile.Campaigns.Count);
            Console.WriteLine ("# of levels: {0}", levfile.Levels.Count);

            levfile.Campaigns.Count.Should ().Be (9);
            levfile.Campaigns [0].Name.Should ().Be ("MULTI_CAM_1");
            levfile.Campaigns [0].Data [3].Should ().Be ("wrf/audio.wrf");
            levfile.Campaigns [4].Data [0].Should ().Be ("wrf/vidmem2.wrf");
            levfile.Levels.Count.Should ().Be (114);
        }

        [Test]
        public void CanParseTinnyWarLev()
        {
            var file = Path.Combine ("Data", 
                                     Path.Combine ("Levels", 
                                     Path.Combine ("2c-Tinny-War.addon.lev")));

            var txt = File.ReadAllText( file );
            Console.WriteLine( "Parsing: {0}", file );
            var levfile = LevGrammar.Lev.Parse (txt);

            Console.WriteLine ("# of campaigns: {0}", levfile.Campaigns.Count);
            Console.WriteLine ("# of levels: {0}", levfile.Levels.Count);

            levfile.Campaigns.Count.Should ().Be (0);
            levfile.Levels.Count.Should ().Be (3);
            levfile.Levels [0].Name.Should ().Be ("Tinny-War-T1");
            levfile.Levels [0].Players.Should ().Be (2);
            levfile.Levels [0].Type.Should ().Be (14);
            levfile.Levels [0].Dataset.Should ().Be ("MULTI_CAM_1");
            levfile.Levels [0].Game.Should ().Be ("multiplay/maps/2c-Tinny-War.gam");
            levfile.Levels [0].Data.Count.Should ().Be (2);
            levfile.Levels [0].Data [0].Should ().Be ("wrf/multi/skirmish2.wrf");
            levfile.Levels [0].Data [1].Should ().Be ("wrf/multi/fog1.wrf");
            levfile.Levels [1].Name.Should ().Be ("Tinny-War-T2");
            levfile.Levels [2].Name.Should ().Be ("Tinny-War-T3");
        }

        [Test]
        [Explicit]
        public void bechmark()
        {
            var file = "Data"
                .CombinePathWith("Levels")
                .CombinePathWith("addon.lev");

            var txt = File.ReadAllText( file );
            Console.WriteLine( "Parsing: {0}", file );
            var s = new Stopwatch();

            s.Start();
            for( int i = 0; i < 1000; i++)
            {
                LevGrammar.Lev.Parse(txt);
            }
            s.Stop();
            Console.WriteLine("Total Time: " + s.Elapsed);       
            //CPU: INTEL 3960X EE, 64GB RAM
            //Total Time: 00:00:39.4012016

        }
    }
}