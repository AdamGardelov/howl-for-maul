#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace FrostMaze.Tests
{
    public sealed class WeaponAudioTests
    {
        [Test,Category("HearthAudio")]
        public void WardbellHasSoftEdgesFiniteStereoAndControlledPeak()
        {
            var clip=(AudioClip)typeof(CombatFeedback).GetMethod("CreateWardbell",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static).Invoke(null,null);
            try {
                Assert.That(clip.channels,Is.EqualTo(2));Assert.That(clip.frequency,Is.EqualTo(48000));
                var data=new float[clip.samples*2];Assert.That(clip.GetData(data,0),Is.True);double energy=0,stereo=0,mean=0;float peak=0;
                for(int i=0;i<data.Length;i++){Assert.That(float.IsNaN(data[i])||float.IsInfinity(data[i]),Is.False);peak=Mathf.Max(peak,Mathf.Abs(data[i]));energy+=data[i]*data[i];mean+=data[i];if(i%2==0)stereo+=Math.Abs(data[i]-data[i+1]);}
                Assert.That(peak,Is.InRange(.15f,.7f));Assert.That(Math.Sqrt(energy/data.Length),Is.InRange(.025,.15));
                Assert.That(Math.Abs(mean/data.Length),Is.LessThan(.001));Assert.That(stereo,Is.GreaterThan(1));
                Assert.That(data[0],Is.EqualTo(0));Assert.That(Math.Abs(data[data.Length-1]),Is.LessThan(.00001));

            } finally { UnityEngine.Object.DestroyImmediate(clip); }
        }
        [UnityTest,Category("MaterialAudio")]
        public IEnumerator MaterialWeaponBankIsDistinctBoundedAndReleasesClips()
        {
            int total=0;var hashes=new HashSet<long>();
            foreach(string map in new[]{"Rimewatch","Ironfold"}) {
                var config=Resources.Load<MapDefinition>(map).Settings;var bank=new TowerSoundBank(config);var held=new List<AudioClip>();var preview=new List<float>();var volley=new float[TowerSoundBank.SampleRate*2*5];
                for(int design=0;design<config.Catalog.Length;design++)for(int variation=0;variation<TowerSoundBank.Variations;variation++) {
                    var clip=bank.Get(design,variation);Assert.That(bank.Get(design,variation),Is.SameAs(clip));held.Add(clip);total++;
                    Assert.That(clip.channels,Is.EqualTo(2));Assert.That(clip.frequency,Is.EqualTo(48000));
                    var samples=new float[clip.samples*clip.channels];Assert.That(clip.GetData(samples,0),Is.True);double energy=0,mean=0;float peak=0;long hash=17;
                    foreach(float sample in samples){Assert.That(float.IsNaN(sample)||float.IsInfinity(sample),Is.False);peak=Mathf.Max(peak,Mathf.Abs(sample));mean+=sample;energy+=sample*sample;unchecked{hash=hash*31+Mathf.RoundToInt(sample*100000);}}
                    Assert.That(peak,Is.InRange(.1f,.85f));Assert.That(Math.Sqrt(energy/samples.Length),Is.InRange(.015,.17));Assert.That(Math.Abs(mean/samples.Length),Is.LessThan(.005));
                    Assert.That(samples[0],Is.Zero.Within(.0001));Assert.That(samples[samples.Length-1],Is.Zero.Within(.0001));Assert.That(hashes.Add(hash),Is.True,"Repeated sound or variation: "+clip.name);
                    if(variation==0&&config.Catalog[design].Spec.Damage>0&&(design%(map=="Rimewatch"?5:7)==0||design%(map=="Rimewatch"?5:7)==2)) {
                        foreach(float sample in samples)preview.Add(sample*.65f);preview.AddRange(new float[48000/4*2]);
                    }
                    if(config.Catalog[design].Spec.Damage>0) {
                        int start=(design*3+variation)%28*(48000*2*12/100);
                        for(int i=0;i<samples.Length&&start+i<volley.Length;i++)volley[start+i]+=samples[i]*.12f;
                    }
                }
                Assert.That(bank.Count,Is.EqualTo(config.Catalog.Length*3));Assert.That(bank.Get(-1),Is.Null);Assert.That(bank.Get(config.Catalog.Length),Is.Null);
                WriteWave("/tmp/Howl-"+map+"-Weapon-Preview.wav",preview.ToArray());
                float mixPeak=0;foreach(float sample in volley)mixPeak=Mathf.Max(mixPeak,Mathf.Abs(sample));Assert.That(mixPeak,Is.LessThan(.95f),"Preview volley clips");
                WriteWave("/tmp/Howl-"+map+"-Weapon-Volley.wav",volley);
                bank.Dispose();yield return null;Assert.That(bank.Count,Is.Zero);foreach(var clip in held)Assert.That(clip==null,Is.True,"Sound bank leaked an owned clip");
            }
            Assert.That(total,Is.EqualTo(228));
        }
        static void WriteWave(string path,float[] samples)
        {
            using(var writer=new BinaryWriter(File.Create(path))){int bytes=samples.Length*2;
                writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));writer.Write(36+bytes);writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));
                writer.Write(16);writer.Write((short)1);writer.Write((short)2);writer.Write(48000);writer.Write(48000*4);writer.Write((short)4);writer.Write((short)16);
                writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));writer.Write(bytes);foreach(float sample in samples)writer.Write((short)Mathf.RoundToInt(Mathf.Clamp(sample,-1,1)*32767));}
        }
    }
}
#endif
