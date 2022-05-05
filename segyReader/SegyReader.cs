using System;
using System.IO;

namespace segyViewer.segyReader;

public class SegyReader
{
        public SegyHeader segyHeader { get; set; }

        public Trace[] traces { get; set; }

        public SegyReader(string filePath)
        {
            segyHeader = new SegyHeader();
            FileStream fs = new FileStream(filePath, FileMode.Open);

            //跳过文件头的前3200字节
            fs.Seek(3200, SeekOrigin.Begin);

            //作业标识号(job identification number)
            byte[] jobIDByte = new byte[4];
            fs.Read(jobIDByte, 0, 4);
            segyHeader.jobID = Byte2Int(jobIDByte);

            //测线号(line number---only one line per reel)
            byte[] lineNumberByte = new byte[4];
            fs.Read(lineNumberByte, 0, 4);
            segyHeader.lineNumber = Byte2Int(lineNumberByte);

            //卷号(reel number)
            byte[] reelNumberByte = new byte[4];
            fs.Read(reelNumberByte, 0, 4);
            segyHeader.reelNumber = Byte2Int(reelNumberByte);

            //每个道集的数据道数(number of data traces per record)
            byte[] ntrprByte = new byte[2];
            fs.Read(ntrprByte, 0, 2);
            segyHeader.ntrpr = Byte2Short(ntrprByte);

            //每个道集的辅助道数(number of auxiliary traces per record)
            byte[] nartByte = new byte[2];
            fs.Read(nartByte, 0, 2);
            segyHeader.nart = Byte2Short(nartByte);

            //微秒（us）形式的采样间隔(sample interval in micro secs for this reel)
            byte[] sampleIntervalByte = new byte[2];
            fs.Read(sampleIntervalByte, 0, 2);
            segyHeader.sampleInterval = Byte2Short(sampleIntervalByte);

            //微秒（us）形式的原始野外记录采样间隔(Sampling interval of original field record)
            byte[] sampleIntervalForOriginalFieldRecordByte = new byte[2];
            fs.Read(sampleIntervalForOriginalFieldRecordByte, 0, 2);
            segyHeader.sampleIntervalForOriginalFieldRecord = Byte2Short(sampleIntervalForOriginalFieldRecordByte);

            //数据道采样点数(number of samples per trace for this reel)
            byte[] samplesByte = new byte[2];
            fs.Read(samplesByte, 0, 2);
            segyHeader.samples = Byte2Short(samplesByte);

            //原始野外记录每道采样点数(Sampling point number of original field record)
            byte[] samplesForOriginalFieldRecordByte = new byte[2];
            fs.Read(samplesForOriginalFieldRecordByte, 0, 2);
            segyHeader.samplesForOriginalFieldRecord = Byte2Short(samplesForOriginalFieldRecordByte);

            /* 数据采样格式编码
            data sample format code:
				1 = floating point (4 bytes)
				2 = fixed point (4 bytes)
				3 = fixed point (2 bytes)
				4 = fixed point w/gain code (4 bytes) */
            byte[] dataSampleFormatByte = new byte[2];
            fs.Read(dataSampleFormatByte, 0, 2);
            segyHeader.dataSampleFormat = Byte2Short(dataSampleFormatByte);

            //道集覆盖次数---每个数据集的期望数据道数(CDP fold expected per CDP ensemble)
            byte[] CDPFoldNumByte = new byte[2];
            fs.Read(CDPFoldNumByte, 0, 2);
            segyHeader.CDPFoldNum = Byte2Short(CDPFoldNumByte);

            /* 道分选码,即集合类型(trace sorting code):
				1 = as recorded (no sorting)
				2 = CDP ensemble
				3 = single fold continuous profile
				4 = horizontally stacked */
            byte[] traceSortingCodeByte = new byte[2];
            fs.Read(traceSortingCodeByte, 0, 2);
            segyHeader.traceSortingCode = Byte2Short(traceSortingCodeByte);

            /* 垂直求和码(vertical sum code):
                1 = no sum
                2 = two sum
                ...
                N = N sum (N = 32,767) */
            byte[] verticalSumCodeByte = new byte[2];
            fs.Read(verticalSumCodeByte, 0, 2);
            segyHeader.verticalSumCode = Byte2Short(verticalSumCodeByte);

            //起始扫描频率(sweep frequency at start)
            byte[] sweepFrequencyAtStartByte = new byte[2];
            fs.Read(sweepFrequencyAtStartByte, 0, 2);
            segyHeader.sweepFrequencyAtStart = Byte2Short(sweepFrequencyAtStartByte);

            //终止扫描频率(sweep frequency at end)
            byte[] sweepFrequencyAtEndByte = new byte[2];
            fs.Read(sweepFrequencyAtEndByte, 0, 2);
            segyHeader.sweepFrequencyAtEnd = Byte2Short(sweepFrequencyAtEndByte);

            //扫描长度，单位ms(sweep length)
            byte[] sweepLengthByte = new byte[2];
            fs.Read(sweepLengthByte, 0, 2);
            segyHeader.sweepLength = Byte2Short(sweepLengthByte);

            /* 扫描类型码(sweep type code):
				1 = linear
				2 = parabolic
				3 = exponential
				4 = other */
            byte[] sweepTypeCodeByte = new byte[2];
            fs.Read(sweepTypeCodeByte, 0, 2);
            segyHeader.sweepTypeCode = Byte2Short(sweepTypeCodeByte);

            //扫描信道的道数(trace number of sweep channel)
            byte[] traceNumberOfSweepChannelByte = new byte[2];
            fs.Read(traceNumberOfSweepChannelByte, 0, 2);
            segyHeader.traceNumberOfSweepChannel = Byte2Short(traceNumberOfSweepChannelByte);

            //有斜坡时，以毫秒表示的扫描道起始斜坡长度
            /* sweep trace taper length at start if
			   tapered (the taper starts at zero time
			   and is effective for this length) */
            byte[] sweepTraceTaperLengthAtStartByte = new byte[2];
            fs.Read(sweepTraceTaperLengthAtStartByte, 0, 2);
            segyHeader.sweepTraceTaperLengthAtStart = Byte2Short(sweepTraceTaperLengthAtStartByte);

            //以毫秒表示的扫描道终止斜坡长度
            /* sweep trace taper length at end (the ending
			   taper starts at sweep length minus the taper
			   length at end) */
            byte[] sweepTraceTaperLengthAtEndByte = new byte[2];
            fs.Read(sweepTraceTaperLengthAtEndByte, 0, 2);
            segyHeader.sweepTraceTaperLengthAtEnd = Byte2Short(sweepTraceTaperLengthAtEndByte);

            /* 斜坡类型(sweep trace taper type code):
				1 = linear
				2 = cos-squared
				3 = other */
            byte[] sweepTraceTaperTypeCodeByte = new byte[2];
            fs.Read(sweepTraceTaperTypeCodeByte, 0, 2);
            segyHeader.sweepTraceTaperTypeCode = Byte2Short(sweepTraceTaperTypeCodeByte);

            /* 相关数据道(correlated data traces code):
                1 = no
				2 = yes */
            byte[] correlatedDataTracesCodeByte = new byte[2];
            fs.Read(correlatedDataTracesCodeByte, 0, 2);
            segyHeader.correlatedDataTracesCode = Byte2Short(correlatedDataTracesCodeByte);

            /* 二进制增益恢复(binary gain recovered code):
                1 = no
				2 = yes */
            byte[] binaryGainRecoveredCodeByte = new byte[2];
            fs.Read(binaryGainRecoveredCodeByte, 0, 2);
            segyHeader.binaryGainRecoveredCode = Byte2Short(binaryGainRecoveredCodeByte);

            /* 振幅恢复方法(amplitude recovery method code):
				1 = none
				2 = spherical divergence
				3 = AGC
				4 = other */
            byte[] amplitudeRecoveryMethodCodeByte = new byte[2];
            fs.Read(amplitudeRecoveryMethodCodeByte, 0, 2);
            segyHeader.amplitudeRecoveryMethodCode = Byte2Short(amplitudeRecoveryMethodCodeByte);

            /* 测量系统(measurement system code):
				1 = meters
				2 = feet */
            byte[] measurementSystemCodeByte = new byte[2];
            fs.Read(measurementSystemCodeByte, 0, 2);
            segyHeader.measurementSystemCode = Byte2Short(measurementSystemCodeByte);

            /* 脉冲极化码(impulse signal polarity code):
				1 = increase in pressure or upward
				    geophone case movement gives
				    negative number on tape
				2 = increase in pressure or upward
				    geophone case movement gives
				    positive number on tape */
            byte[] impulseSignalPolarityCodeByte = new byte[2];
            fs.Read(impulseSignalPolarityCodeByte, 0, 2);
            segyHeader.impulseSignalPolarityCode = Byte2Short(impulseSignalPolarityCodeByte);

            /* 可控源极化码(vibratory polarity code):
				code	seismic signal lags pilot by
				1	337.5 to  22.5 degrees
				2	 22.5 to  67.5 degrees
				3	 67.5 to 112.5 degrees
				4	112.5 to 157.5 degrees
				5	157.5 to 202.5 degrees
				6	202.5 to 247.5 degrees
				7	247.5 to 292.5 degrees
				8	293.5 to 337.5 degrees */
            byte[] vibratoryPolarityCodeByte = new byte[2];
            fs.Read(vibratoryPolarityCodeByte, 0, 2);
            segyHeader.vibratoryPolarityCode = Byte2Short(vibratoryPolarityCodeByte);

            // 跳过文件头的后340个字节
            fs.Seek(340, SeekOrigin.Current);

            //
            traces = new Trace[segyHeader.ntrpr];
            for (int i = 0; i < segyHeader.ntrpr; i++)
            {
                traces[i] = new Trace();
                //测线中道顺序号
                byte[] traceSequenceNumberWithinLineByte = new byte[4];
                fs.Read(traceSequenceNumberWithinLineByte, 0, 4);
                traces[i].traceHeader.traceSequenceNumberWithinLine = Byte2Int(traceSequenceNumberWithinLineByte);

                //SEG-Y文件中道顺序号
                byte[] traceSequenceNumberWithinReelByte = new byte[4];
                fs.Read(traceSequenceNumberWithinReelByte, 0, 4);
                traces[i].traceHeader.traceSequenceNumberWithinReel = Byte2Int(traceSequenceNumberWithinReelByte);

                //野外原始记录号
                byte[] fieldRecordNumberByte = new byte[4];
                fs.Read(fieldRecordNumberByte, 0, 4);
                traces[i].traceHeader.fieldRecordNumber = Byte2Int(fieldRecordNumberByte);

                //野外原始记录的道号
                byte[] traceNumberWithinFieldRecordByte = new byte[4];
                fs.Read(traceNumberWithinFieldRecordByte, 0, 4);
                traces[i].traceHeader.traceNumberWithinFieldRecord = Byte2Int(traceNumberWithinFieldRecordByte);

                //震源点号
                byte[] energySourcePointNumberByte = new byte[4];
                fs.Read(energySourcePointNumberByte, 0, 4);
                traces[i].traceHeader.energySourcePointNumber = Byte2Int(energySourcePointNumberByte);

                //道集号
                byte[] CDPEnsembleNumberByte = new byte[4];
                fs.Read(CDPEnsembleNumberByte, 0, 4);
                traces[i].traceHeader.CDPEnsembleNumber = Byte2Int(CDPEnsembleNumberByte);

                //道集的道数
                byte[] traceNumberWithinCDPEnsembleByte = new byte[4];
                fs.Read(traceNumberWithinCDPEnsembleByte, 0, 4);
                traces[i].traceHeader.traceNumberWithinCDPEnsemble = Byte2Int(traceNumberWithinCDPEnsembleByte);

                //道识别码
                byte[] traceIdentificationCodeByte = new byte[2];
                fs.Read(traceIdentificationCodeByte, 0, 2);
                traces[i].traceHeader.traceIdentificationCode = Byte2Short(traceIdentificationCodeByte);

                //产生该道的垂直叠加道数
                byte[] numbeOfVerticallySummedTracesByte = new byte[2];
                fs.Read(numbeOfVerticallySummedTracesByte, 0, 2);
                traces[i].traceHeader.numbeOfVerticallySummedTraces = Byte2Short(numbeOfVerticallySummedTracesByte);

                //产生该道的水平叠加道数
                byte[] numbeOfHorizontallySummedTracesByte = new byte[2];
                fs.Read(numbeOfHorizontallySummedTracesByte, 0, 2);
                traces[i].traceHeader.numbeOfHorizontallySummedTraces = Byte2Short(numbeOfHorizontallySummedTracesByte);

                //数据用途
                byte[] dataUseByte = new byte[2];
                fs.Read(dataUseByte, 0, 2);
                traces[i].traceHeader.dataUse = Byte2Short(dataUseByte);

                //从震源中心点到检波器组中心的距离（若与炮激发线方向相反取负）
                byte[] distanceFromSourcePoint2ReceiverGroupByte = new byte[4];
                fs.Read(distanceFromSourcePoint2ReceiverGroupByte, 0, 4);
                traces[i].traceHeader.distanceFromSourcePoint2ReceiverGroup = Byte2Int(distanceFromSourcePoint2ReceiverGroupByte);

                //检波器组高程（所有基准以上高程为正，以下为负）
                byte[] receiverGroupElevationFromSeaLevelByte = new byte[4];
                fs.Read(receiverGroupElevationFromSeaLevelByte, 0, 4);
                traces[i].traceHeader.receiverGroupElevationFromSeaLevel = Byte2Int(receiverGroupElevationFromSeaLevelByte);

                //震源地表高程
                byte[] sourceElevationFromSeaLevelByte = new byte[4];
                fs.Read(sourceElevationFromSeaLevelByte, 0, 4);
                traces[i].traceHeader.sourceElevationFromSeaLevel = Byte2Int(sourceElevationFromSeaLevelByte);

                //震源距地表深度（正数）
                byte[] sourceDepthByte = new byte[4];
                fs.Read(sourceDepthByte, 0, 4);
                traces[i].traceHeader.sourceDepth = Byte2Int(sourceDepthByte);

                //检波器组基准高程
                byte[] datumElevationAtReceiverGroupByte = new byte[4];
                fs.Read(datumElevationAtReceiverGroupByte, 0, 4);
                traces[i].traceHeader.datumElevationAtReceiverGroup = Byte2Int(datumElevationAtReceiverGroupByte);

                //震源基准高程
                byte[] datumElevationAtSourceByte = new byte[4];
                fs.Read(datumElevationAtSourceByte, 0, 4);
                traces[i].traceHeader.datumElevationAtSource = Byte2Int(datumElevationAtSourceByte);

                //震源水深
                byte[] waterDepthAtSourceByte = new byte[4];
                fs.Read(waterDepthAtSourceByte, 0, 4);
                traces[i].traceHeader.waterDepthAtSource = Byte2Int(waterDepthAtSourceByte);

                //检波器组水深
                byte[] waterDepthAtReceiverGroupByte = new byte[4];
                fs.Read(waterDepthAtReceiverGroupByte, 0, 4);
                traces[i].traceHeader.waterDepthAtReceiverGroup = Byte2Int(waterDepthAtReceiverGroupByte);

                //应用于所有在道头41-68字节给定的真实高程和深度的因子
                byte[] scalelByte = new byte[2];
                fs.Read(scalelByte, 0, 2);
                traces[i].traceHeader.scalel = Byte2Short(scalelByte);

                //应用于所有在道头73-88字节和181-188字节给定的真实坐标值的因子
                byte[] scalcoByte = new byte[2];
                fs.Read(scalcoByte, 0, 2);
                traces[i].traceHeader.scalco = Byte2Short(scalcoByte);

                //震源坐标--X
                byte[] XSourceCoordinateByte = new byte[4];
                fs.Read(XSourceCoordinateByte, 0, 4);
                traces[i].traceHeader.XSourceCoordinate = Byte2Int(XSourceCoordinateByte);

                //震源坐标--Y
                byte[] YSourceCoordinateByte = new byte[4];
                fs.Read(YSourceCoordinateByte, 0, 4);
                traces[i].traceHeader.YSourceCoordinate = Byte2Int(YSourceCoordinateByte);

                //检波器组坐标--X
                byte[] XGroupCoordinateByte = new byte[4];
                fs.Read(XGroupCoordinateByte, 0, 4);
                traces[i].traceHeader.XGroupCoordinate = Byte2Int(XGroupCoordinateByte);

                //检波器组坐标--Y
                byte[] YGroupCoordinateByte = new byte[4];
                fs.Read(YGroupCoordinateByte, 0, 4);
                traces[i].traceHeader.YGroupCoordinate = Byte2Int(YGroupCoordinateByte);

                //坐标单位
                byte[] coordinateUnitsCodeByte = new byte[2];
                fs.Read(coordinateUnitsCodeByte, 0, 2);
                traces[i].traceHeader.coordinateUnitsCode = Byte2Short(coordinateUnitsCodeByte);

                //风化层速度
                byte[] weatheringVelocityByte = new byte[2];
                fs.Read(weatheringVelocityByte, 0, 2);
                traces[i].traceHeader.weatheringVelocity = Byte2Short(weatheringVelocityByte);

                //风化层下速度
                byte[] subweatheringVelocityByte = new byte[2];
                fs.Read(subweatheringVelocityByte, 0, 2);
                traces[i].traceHeader.subweatheringVelocity = Byte2Short(subweatheringVelocityByte);

                //震源处井口时间（毫秒）
                byte[] upholeTimeAtSourceByte = new byte[2];
                fs.Read(upholeTimeAtSourceByte, 0, 2);
                traces[i].traceHeader.upholeTimeAtSource = Byte2Short(upholeTimeAtSourceByte);

                //检波器组处井口时间（毫秒）
                byte[] upholeTimeAtReceiverGroupByte = new byte[2];
                fs.Read(upholeTimeAtReceiverGroupByte, 0, 2);
                traces[i].traceHeader.upholeTimeAtReceiverGroup = Byte2Short(upholeTimeAtReceiverGroupByte);

                //震源的静校正量（毫秒）
                byte[] sourceStaticCorrectionByte = new byte[2];
                fs.Read(sourceStaticCorrectionByte, 0, 2);
                traces[i].traceHeader.sourceStaticCorrection = Byte2Short(sourceStaticCorrectionByte);

                //检波器组的校正量（毫秒）
                byte[] groupStaticCorrectionByte = new byte[2];
                fs.Read(groupStaticCorrectionByte, 0, 2);
                traces[i].traceHeader.groupStaticCorrection = Byte2Short(groupStaticCorrectionByte);

                //应用的总静校正量（毫秒）（如没有应用静校正量为零）
                byte[] totalStaticAppliedByte = new byte[2];
                fs.Read(totalStaticAppliedByte, 0, 2);
                traces[i].traceHeader.totalStaticApplied = Byte2Short(totalStaticAppliedByte);

                //延迟时间A
                byte[] lagTimeAByte = new byte[2];
                fs.Read(lagTimeAByte, 0, 2);
                traces[i].traceHeader.lagTimeA = Byte2Short(lagTimeAByte);

                //延迟时间B
                byte[] lagTimeBByte = new byte[2];
                fs.Read(lagTimeBByte, 0, 2);
                traces[i].traceHeader.lagTimeB = Byte2Short(lagTimeBByte);

                //记录延迟时间
                byte[] delayRecordingTimeByte = new byte[2];
                fs.Read(delayRecordingTimeByte, 0, 2);
                traces[i].traceHeader.delayRecordingTime = Byte2Short(delayRecordingTimeByte);

                //起始切除时间（毫秒）
                byte[] muteTimeStartByte = new byte[2];
                fs.Read(muteTimeStartByte, 0, 2);
                traces[i].traceHeader.muteTimeStart = Byte2Short(muteTimeStartByte);

                //终止切除时间（毫秒）
                byte[] muteTimeEndByte = new byte[2];
                fs.Read(muteTimeEndByte, 0, 2);
                traces[i].traceHeader.muteTimeEnd = Byte2Short(muteTimeEndByte);

                //该道采样点数
                byte[] numberOfSamplesByte = new byte[2];
                fs.Read(numberOfSamplesByte, 0, 2);
                traces[i].traceHeader.numberOfSamples = Byte2Short(numberOfSamplesByte);

                //该道采样间隔（微秒）
                byte[] traceSampleIntervalByte = new byte[2];
                fs.Read(traceSampleIntervalByte, 0, 2);
                traces[i].traceHeader.sampleInterval = Byte2Short(traceSampleIntervalByte);

                //野外仪器增益类型
                byte[] gainTypeByte = new byte[2];
                fs.Read(gainTypeByte, 0, 2);
                traces[i].traceHeader.gainType = Byte2Short(gainTypeByte);

                //仪器增益常数（分贝）
                byte[] instrumentGainConstantByte = new byte[2];
                fs.Read(instrumentGainConstantByte, 0, 2);
                traces[i].traceHeader.instrumentGainConstant = Byte2Short(instrumentGainConstantByte);

                //仪器初始增益（分贝）
                byte[] instrumentEarlyOrInitialGainByte = new byte[2];
                fs.Read(instrumentEarlyOrInitialGainByte, 0, 2);
                traces[i].traceHeader.instrumentEarlyOrInitialGain = Byte2Short(instrumentEarlyOrInitialGainByte);

                //相关
                byte[] correlatedByte = new byte[2];
                fs.Read(correlatedByte, 0, 2);
                traces[i].traceHeader.correlated = Byte2Short(correlatedByte);

                //起始扫描频率（赫兹）
                byte[] traceSweepFrequencyAtStartByte = new byte[2];
                fs.Read(traceSweepFrequencyAtStartByte, 0, 2);
                traces[i].traceHeader.sweepFrequencyAtStart = Byte2Short(traceSweepFrequencyAtStartByte);

                //终止扫描频率（赫兹）
                byte[] traceSweepFrequencyAtEndByte = new byte[2];
                fs.Read(traceSweepFrequencyAtEndByte, 0, 2);
                traces[i].traceHeader.sweepFrequencyAtEnd = Byte2Short(traceSweepFrequencyAtEndByte);

                //扫描长度（毫秒）
                byte[] traceSweepLengthByte = new byte[2];
                fs.Read(traceSweepLengthByte, 0, 2);
                traces[i].traceHeader.sweepLength = Byte2Short(traceSweepLengthByte);

                //扫描类型
                byte[] traceSweepTypeByte = new byte[2];
                fs.Read(traceSweepTypeByte, 0, 2);
                traces[i].traceHeader.sweepType = Byte2Short(traceSweepTypeByte);

                //扫描道斜坡起始长度（毫秒）
                byte[] sweepTraceLengthAtStartByte = new byte[2];
                fs.Read(sweepTraceLengthAtStartByte, 0, 2);
                traces[i].traceHeader.sweepTraceLengthAtStart = Byte2Short(sweepTraceLengthAtStartByte);

                //扫描道斜坡终止长度（毫秒）
                byte[] sweepTraceLengthAtEndByte = new byte[2];
                fs.Read(sweepTraceLengthAtEndByte, 0, 2);
                traces[i].traceHeader.sweepTraceLengthAtEnd = Byte2Short(sweepTraceLengthAtEndByte);

                //斜坡类型
                byte[] taperTypeByte = new byte[2];
                fs.Read(taperTypeByte, 0, 2);
                traces[i].traceHeader.taperType = Byte2Short(taperTypeByte);

                //假频滤波频率（赫兹）
                byte[] aliasFilterFrequencyByte = new byte[2];
                fs.Read(aliasFilterFrequencyByte, 0, 2);
                traces[i].traceHeader.aliasFilterFrequency = Byte2Short(aliasFilterFrequencyByte);

                //假频滤波坡度（分贝/倍频程）
                byte[] aliasFilterSlopeByte = new byte[2];
                fs.Read(aliasFilterSlopeByte, 0, 2);
                traces[i].traceHeader.aliasFilterSlope = Byte2Short(aliasFilterSlopeByte);

                //陷波频率（赫兹）
                byte[] notchFilterFrequencyByte = new byte[2];
                fs.Read(notchFilterFrequencyByte, 0, 2);
                traces[i].traceHeader.notchFilterFrequency = Byte2Short(notchFilterFrequencyByte);

                //陷波坡度（分贝/倍频程）
                byte[] notchFilterSlopeByte = new byte[2];
                fs.Read(notchFilterSlopeByte, 0, 2);
                traces[i].traceHeader.notchFilterSlope = Byte2Short(notchFilterSlopeByte);

                //低截频率（赫兹）
                byte[] lowCutFrequencyByte = new byte[2];
                fs.Read(lowCutFrequencyByte, 0, 2);
                traces[i].traceHeader.lowCutFrequency = Byte2Short(lowCutFrequencyByte);

                //高截频率（赫兹）
                byte[] highCutFrequencyByte = new byte[2];
                fs.Read(highCutFrequencyByte, 0, 2);
                traces[i].traceHeader.highCutFrequency = Byte2Short(highCutFrequencyByte);

                //低截坡度（分贝/倍频程）
                byte[] lowCutSlopeByte = new byte[2];
                fs.Read(lowCutSlopeByte, 0, 2);
                traces[i].traceHeader.lowCutSlope = Byte2Short(lowCutSlopeByte);

                //高截坡度（分贝/倍频程）
                byte[] highCutSlopeByte = new byte[2];
                fs.Read(highCutSlopeByte, 0, 2);
                traces[i].traceHeader.highCutSlope = Byte2Short(highCutSlopeByte);

                //数据记录的年份
                byte[] yearByte = new byte[2];
                fs.Read(yearByte, 0, 2);
                traces[i].traceHeader.year = Byte2Short(yearByte);

                //数据记录的日期
                byte[] dayByte = new byte[2];
                fs.Read(dayByte, 0, 2);
                traces[i].traceHeader.day = Byte2Short(dayByte);

                //数据记录的时间
                byte[] hourByte = new byte[2];
                fs.Read(hourByte, 0, 2);
                traces[i].traceHeader.hour = Byte2Short(hourByte);

                //数据记录的分钟
                byte[] minuteByte = new byte[2];
                fs.Read(minuteByte, 0, 2);
                traces[i].traceHeader.minute = Byte2Short(minuteByte);

                //数据记录的秒
                byte[] secondByte = new byte[2];
                fs.Read(secondByte, 0, 2);
                traces[i].traceHeader.second = Byte2Short(secondByte);

                //时间基准码
                byte[] timeBasisCodeByte = new byte[2];
                fs.Read(timeBasisCodeByte, 0, 2);
                traces[i].traceHeader.timeBasisCode = Byte2Short(timeBasisCodeByte);

                //道加权因子
                byte[] traceWeightingFactorByte = new byte[2];
                fs.Read(traceWeightingFactorByte, 0, 2);
                traces[i].traceHeader.traceWeightingFactor = Byte2Short(traceWeightingFactorByte);

                //滚动开关位置1的检波器组号
                byte[] grnorsByte = new byte[2];
                fs.Read(grnorsByte, 0, 2);
                traces[i].traceHeader.grnors = Byte2Short(grnorsByte);

                //野外原始记录中道号1的检波器组号
                byte[] grnofrByte = new byte[2];
                fs.Read(grnofrByte, 0, 2);
                traces[i].traceHeader.grnofr = Byte2Short(grnofrByte);

                //野外原始记录中最后一道的检波器组号
                byte[] grnlofByte = new byte[2];
                fs.Read(grnlofByte, 0, 2);
                traces[i].traceHeader.grnlof = Byte2Short(grnlofByte);

                //间隔大小
                byte[] gapSizeByte = new byte[2];
                fs.Read(gapSizeByte, 0, 2);
                traces[i].traceHeader.gapSize = Byte2Short(gapSizeByte);

                //相对测线斜坡起始或终止点的移动
                byte[] overtravelTaperCodeByte = new byte[2];
                fs.Read(overtravelTaperCodeByte, 0, 2);
                traces[i].traceHeader.overtravelTaperCode = Byte2Short(overtravelTaperCodeByte);

                fs.Seek(60, SeekOrigin.Current);

                traces[i].values = new double[traces[i].traceHeader.numberOfSamples];
                if (segyHeader.dataSampleFormat == 1)
                {
                    for (int j = 0; j < traces[i].traceHeader.numberOfSamples; j++)
                    {
                        byte[] dataByte = new byte[4];
                        fs.Read(dataByte, 0, 4);
                        traces[i].values[j] = Byte2Float(dataByte);
                    }
                }
                else if (segyHeader.dataSampleFormat == 2)
                {
                    for (int j = 0; j < traces[i].traceHeader.numberOfSamples; j++)
                    {
                        byte[] dataByte = new byte[4];
                        fs.Read(dataByte, 0, 4);
                        traces[i].values[j] = Byte2Int(dataByte);
                    }
                }
            }
            fs.Close();
        }

        private short Byte2Short(byte[] byteArr)
        {
            Array.Reverse(byteArr);
            return BitConverter.ToInt16(byteArr, 0);
        }

        private int Byte2Int(byte[] byteArr)
        {
            Array.Reverse(byteArr);
            return BitConverter.ToInt32(byteArr, 0);
        }

        private float Byte2Float(byte[] byteArr)
        {
            Array.Reverse(byteArr);
            return BitConverter.ToSingle(byteArr, 0);
        }
    }

    public class SegyHeader
    {
        /// <summary>
        /// 作业标识号
        /// <para>job identification number</para>
        /// </summary>
        public int jobID { get; set; }

        /// <summary>
        /// 测线号，对3-D叠后数据而言，它将典型地包含纵向测线（In-line）号
        /// <para>line number (only one line per reel)</para>
        /// </summary>
        public int lineNumber { get; set; }

        /// <summary>
        /// 卷号
        /// <para>reel number</para>
        /// </summary>
        public int reelNumber { get; set; }

        /// <summary>
        /// 每个道集的数据道数
        /// <para>number of data traces per record</para>
        /// </summary>
        public short ntrpr { get; set; }

        /// <summary>
        /// 每个道集的辅助道数
        /// <para>number of auxiliary traces per record</para>
        /// </summary>
        public short nart { get; set; }

        /// <summary>
        /// 微秒（us）形式的采样间隔
        /// <para>sample interval in micro secs for this reel</para>
        /// </summary>
        public short sampleInterval { get; set; }

        /// <summary>
        /// 微秒（us）形式的原始野外记录采样间隔
        /// <para>sample interval in micro secs for original field recording</para>
        /// </summary>
        public short sampleIntervalForOriginalFieldRecord { get; set; }

        /// <summary>
        /// 数据道采样点数
        /// <para>number of samples per trace for this reel</para>
        /// </summary>
        public short samples { get; set; }

        /// <summary>
        /// 原始野外记录每道采样点数
        /// <para>number of samples per trace for original field recording</para>
        /// </summary>
        public short samplesForOriginalFieldRecord { get; set; }

        /// <summary>
        /// 数据采样格式编码
        /// <para>data sample format code</para>
        /// </summary>
        public short dataSampleFormat { get; set; }

        /// <summary>
        /// 道集覆盖次数（每个数据集的期望数据道数）
        /// <para>CDP fold expected per CDP ensemble</para>
        /// </summary>
        public short CDPFoldNum { get; set; }

        /// <summary>
        /// 道分选码（即集合类型）
        /// <para>trace sorting code</para>
        /// </summary>
        public short traceSortingCode { get; set; }

        /// <summary>
        /// 垂直求和码
        /// <para>vertical sum code</para>
        /// </summary>
        public short verticalSumCode { get; set; }

        /// <summary>
        /// 起始扫描频率（Hz）
        /// <para>sweep frequency at start</para>
        /// </summary>
        public short sweepFrequencyAtStart { get; set; }

        /// <summary>
        /// 终止扫描频率（Hz）
        /// <para>sweep frequency at end</para>
        /// </summary>
        public short sweepFrequencyAtEnd { get; set; }

        /// <summary>
        /// 扫描长度（ms）
        /// <para>sweep length (ms)</para>
        /// </summary>
        public short sweepLength { get; set; }

        /// <summary>
        /// 扫描类型码
        /// <para>sweep type code</para>
        /// </summary>
        public short sweepTypeCode { get; set; }

        /// <summary>
        /// 扫描信道的道数
        /// <para>trace number of sweep channel</para>
        /// </summary>
        public short traceNumberOfSweepChannel { get; set; }

        /// <summary>
        /// 有斜坡时，以毫秒表示的扫描道起始斜坡长度（斜坡从零时刻开始，对这个长度有效）
        /// <para>sweep trace taper length at start if tapered(the taper starts at zero time and is effective for this length)</para>
        /// </summary>
        public short sweepTraceTaperLengthAtStart { get; set; }

        /// <summary>
        /// 以毫秒表示的扫描道终止斜坡长度（斜坡终止始于扫描长度减去斜坡结尾处的长度）
        /// <para>sweep trace taper length at end (the ending taper starts at sweep length minus the taper length at end)</para>
        /// </summary>
        public short sweepTraceTaperLengthAtEnd { get; set; }

        /// <summary>
        /// 斜坡类型
        /// <para>sweep trace taper type code</para>
        /// </summary>
        public short sweepTraceTaperTypeCode { get; set; }

        /// <summary>
        /// 相关数据道
        /// <para>correlated data traces code</para>
        /// </summary>
        public short correlatedDataTracesCode { get; set; }

        /// <summary>
        /// 二进制增益恢复
        /// <para>binary gain recovered code</para>
        /// </summary>
        public short binaryGainRecoveredCode { get; set; }

        /// <summary>
        /// 振幅恢复方法
        /// <para>amplitude recovery method code</para>
        /// </summary>
        public short amplitudeRecoveryMethodCode { get; set; }

        /// <summary>
        /// 测量系统
        /// <para>measurement system code</para>
        /// </summary>
        public short measurementSystemCode { get; set; }

        /// <summary>
        /// 脉冲极化码
        /// <para>impulse signal polarity code</para>
        /// </summary>
        public short impulseSignalPolarityCode { get; set; }

        /// <summary>
        /// 可控源极化码
        /// <para>vibratory polarity code</para>
        /// </summary>
        public short vibratoryPolarityCode { get; set; }
    }

    public class Trace
    {
        public TraceHeader traceHeader { get; set; }
        public double[] values { get; set; }

        public Trace()
        {
            traceHeader = new TraceHeader();
        }
    }

    public class TraceHeader
    {
        /// <summary>
        /// 测线中道顺序号，若一条测线有若干SEG-Y文件号数递增
        /// <para>trace sequence number within line</para>
        /// </summary>
        public int traceSequenceNumberWithinLine { get; set; }

        /// <summary>
        /// SEG-Y文件中道顺序号，每个文件以道顺序1开始
        /// <para>trace sequence number within reel</para>
        /// </summary>
        public int traceSequenceNumberWithinReel { get; set; }

        /// <summary>
        /// 野外原始记录号
        /// <para>field record number</para>
        /// </summary>
        public int fieldRecordNumber { get; set; }

        /// <summary>
        /// 野外原始记录的道号
        /// <para>trace number within field record</para>
        /// </summary>
        public int traceNumberWithinFieldRecord { get; set; }

        /// <summary>
        /// 震源点号，当在相同有效地表位置多于一个记录时使用
        /// <para>energy source point number</para>
        /// </summary>
        public int energySourcePointNumber { get; set; }

        /// <summary>
        /// 道集号（即CDP，CMP，CRP等）
        /// <para>CDP ensemble number</para>
        /// </summary>
        public int CDPEnsembleNumber { get; set; }

        /// <summary>
        /// 道集的道数，每个道集从道号1开始
        /// <para>trace number within CDP ensemble</para>
        /// </summary>
        public int traceNumberWithinCDPEnsemble { get; set; }

        /// <summary>
        /// 道识别码
        /// <para>trace identification code</para>
        /// </summary>
        public short traceIdentificationCode { get; set; }

        /// <summary>
        /// 产生该道的垂直叠加道数
        /// <para>number of vertically summed traces</para>
        /// </summary>
        public short numbeOfVerticallySummedTraces { get; set; }

        /// <summary>
        /// 产生该道的水平叠加道数
        /// <para>number of horizontally summed traces</para>
        /// </summary>
        public short numbeOfHorizontallySummedTraces { get; set; }

        /// <summary>
        /// 数据用途
        /// <para>data use</para>
        /// </summary>
        public short dataUse { get; set; }

        /// <summary>
        /// 从震源中心点到检波器组中心的距离（若与炮激发线方向相反取负）
        /// <para>distance from source point to receiver group</para>
        /// </summary>
        public int distanceFromSourcePoint2ReceiverGroup { get; set; }

        /// <summary>
        /// 检波器组高程（所有基准以上高程为正，以下为负）
        /// <para>receiver group elevation from sea level</para>
        /// </summary>
        public int receiverGroupElevationFromSeaLevel { get; set; }

        /// <summary>
        /// 震源地表高程
        /// <para>source elevation from sea level</para>
        /// </summary>
        public int sourceElevationFromSeaLevel { get; set; }

        /// <summary>
        /// 震源距地表深度（正数）
        /// <para>source depth (positive)</para>
        /// </summary>
        public int sourceDepth { get; set; }

        /// <summary>
        /// 检波器组基准高程
        /// <para>datum elevation at receiver group</para>
        /// </summary>
        public int datumElevationAtReceiverGroup { get; set; }

        /// <summary>
        /// 震源基准高程
        /// <para>datum elevation at source</para>
        /// </summary>
        public int datumElevationAtSource { get; set; }

        /// <summary>
        /// 震源水深
        /// <para>water depth at source</para>
        /// </summary>
        public int waterDepthAtSource { get; set; }

        /// <summary>
        /// 检波器组水深
        /// <para>water depth at receiver group</para>
        /// </summary>
        public int waterDepthAtReceiverGroup { get; set; }

        /// <summary>
        /// 应用于所有在道头41-68字节给定的真实高程和深度的因子
        /// <para>scale factor for previous 7 entries with value plus or minus 10 to the power 0, 1, 2, 3, or 4</para>
        /// </summary>
        public short scalel { get; set; }

        /// <summary>
        /// 应用于所有在道头73-88字节和181-188字节给定的真实坐标值的因子
        /// <para>scale factor for next 4 entries with value plus or minus 10 to the power 0, 1, 2, 3, or 4</para>
        /// </summary>
        public short scalco { get; set; }

        /// <summary>
        /// 震源坐标--X
        /// <para>X source coordinate</para>
        /// </summary>
        public int XSourceCoordinate { get; set; }

        /// <summary>
        /// 震源坐标--Y
        /// <para>Y source coordinate</para>
        /// </summary>
        public int YSourceCoordinate { get; set; }

        /// <summary>
        /// 检波器组坐标--X
        /// <para>X group coordinate</para>
        /// </summary>
        public int XGroupCoordinate { get; set; }

        /// <summary>
        /// 检波器组坐标--Y
        /// <para>Y group coordinate</para>
        /// </summary>
        public int YGroupCoordinate { get; set; }

        /// <summary>
        /// 坐标单位
        /// <para>coordinate units code</para>
        /// </summary>
        public short coordinateUnitsCode { get; set; }

        /// <summary>
        /// 风化层速度
        /// <para>weathering velocity</para>
        /// </summary>
        public short weatheringVelocity { get; set; }

        /// <summary>
        /// 风化层下速度
        /// <para>subweathering velocity</para>
        /// </summary>
        public short subweatheringVelocity { get; set; }

        /// <summary>
        /// 震源处井口时间（毫秒）
        /// <para>uphole time at source</para>
        /// </summary>
        public short upholeTimeAtSource { get; set; }

        /// <summary>
        /// 检波器组处井口时间（毫秒）
        /// <para>uphole time at receiver group</para>
        /// </summary>
        public short upholeTimeAtReceiverGroup { get; set; }

        /// <summary>
        /// 震源的静校正量（毫秒）
        /// <para>source static correction</para>
        /// </summary>
        public short sourceStaticCorrection { get; set; }

        /// <summary>
        /// 检波器组的校正量（毫秒）
        /// <para>group static correction</para>
        /// </summary>
        public short groupStaticCorrection { get; set; }

        /// <summary>
        /// 应用的总静校正量（毫秒）（如没有应用静校正量为零）
        /// <para>total static applied</para>
        /// </summary>
        public short totalStaticApplied { get; set; }

        /// <summary>
        /// 延迟时间A，以毫秒表示的240字节道识别头的结束和时间断点之间的时间。当时间断点出现在头之后，该值为正；当时间断点出现在头之前，该值为负。时间断点是最初脉冲，它由辅助道记录或由其他记录系统指定。
        /// <para>lag time A, time in ms between end of 240-byte trace identification header and time break, positive if time break occurs after end of header, time break is defined asthe initiation pulse which maybe recorded on an auxiliary trace or as otherwise specified by the recording system</para>
        /// </summary>
        public short lagTimeA { get; set; }

        /// <summary>
        /// 延迟时间B，以毫秒表示的时间断点到能量源起爆时间之间的时间，可正可负。
        /// <para>lag time B, time in ms between the time break and the initiation time of the energy source, may be positive or negative</para>
        /// </summary>
        public short lagTimeB { get; set; }

        /// <summary>
        /// 记录延迟时间，以毫秒表示的能量源起爆时间到数据采样开始记录之间的时间。
        /// <para>delay recording time, time in ms between initiation time of energy source and time when recording of data samples begins (for deep water work if recording does not start at zero time)</para>
        /// </summary>
        public short delayRecordingTime { get; set; }

        /// <summary>
        /// 起始切除时间（毫秒）
        /// <para>mute time--start</para>
        /// </summary>
        public short muteTimeStart { get; set; }

        /// <summary>
        /// 终止切除时间（毫秒）
        /// <para>mute time--end</para>
        /// </summary>
        public short muteTimeEnd { get; set; }

        /// <summary>
        /// 该道采样点数
        /// <para>number of samples in this trace</para>
        /// </summary>
        public short numberOfSamples { get; set; }

        /// <summary>
        /// 该道采样间隔（微秒）
        /// <para>sample interval in this trace</para>
        /// </summary>
        public short sampleInterval { get; set; }

        /// <summary>
        /// 野外仪器增益类型
        /// <para>gain type of field instruments code</para>
        /// </summary>
        public short gainType { get; set; }

        /// <summary>
        /// 仪器增益常数（分贝）
        /// <para>instrument gain constant</para>
        /// </summary>
        public short instrumentGainConstant { get; set; }

        /// <summary>
        /// 仪器初始增益（分贝）
        /// <para>instrument early or initial gain</para>
        /// </summary>
        public short instrumentEarlyOrInitialGain { get; set; }

        /// <summary>
        /// 相关
        /// <para>correlated</para>
        /// </summary>
        public short correlated { get; set; }

        /// <summary>
        /// 起始扫描频率（赫兹）
        /// <para>sweep frequency at start</para>
        /// </summary>
        public short sweepFrequencyAtStart { get; set; }

        /// <summary>
        /// 终止扫描频率（赫兹）
        /// <para>sweep frequency at end</para>
        /// </summary>
        public short sweepFrequencyAtEnd { get; set; }

        /// <summary>
        /// 扫描长度（毫秒）
        /// <para>sweep length in ms</para>
        /// </summary>
        public short sweepLength { get; set; }

        /// <summary>
        /// 扫描类型
        /// <para>sweep type code</para>
        /// </summary>
        public short sweepType { get; set; }

        /// <summary>
        /// 扫描道斜坡起始长度（毫秒）
        /// <para>sweep trace length at start in ms</para>
        /// </summary>
        public short sweepTraceLengthAtStart { get; set; }

        /// <summary>
        /// 扫描道斜坡终止长度（毫秒）
        /// <para>sweep trace length at end in ms</para>
        /// </summary>
        public short sweepTraceLengthAtEnd { get; set; }

        /// <summary>
        /// 斜坡类型
        /// <para>taper type</para>
        /// </summary>
        public short taperType { get; set; }

        /// <summary>
        /// 假频滤波频率（赫兹），若使用
        /// <para>alias filter frequency if used</para>
        /// </summary>
        public short aliasFilterFrequency { get; set; }

        /// <summary>
        /// 假频滤波坡度（分贝/倍频程）
        /// <para>alias filter slope</para>
        /// </summary>
        public short aliasFilterSlope { get; set; }

        /// <summary>
        /// 陷波频率（赫兹），若使用
        /// <para>notch filter frequency if used</para>
        /// </summary>
        public short notchFilterFrequency { get; set; }

        /// <summary>
        /// 陷波坡度（分贝/倍频程）
        /// <para>notch filter slope</para>
        /// </summary>
        public short notchFilterSlope { get; set; }

        /// <summary>
        /// 低截频率（赫兹），若使用
        /// <para>low cut frequency if used</para>
        /// </summary>
        public short lowCutFrequency { get; set; }

        /// <summary>
        /// 高截频率（赫兹），若使用
        /// <para>high cut frequency if used</para>
        /// </summary>
        public short highCutFrequency { get; set; }

        /// <summary>
        /// 低截坡度（分贝/倍频程）
        /// <para>low cut slope</para>
        /// </summary>
        public short lowCutSlope { get; set; }

        /// <summary>
        /// 高截坡度（分贝/倍频程）
        /// <para>high cut slope</para>
        /// </summary>
        public short highCutSlope { get; set; }

        /// <summary>
        /// 数据记录的年份
        /// <para>year data recorded</para>
        /// </summary>
        public short year { get; set; }

        /// <summary>
        /// 数据记录的日期
        /// <para>day of year</para>
        /// </summary>
        public short day { get; set; }

        /// <summary>
        /// 数据记录的时（24小时制）
        /// <para>hour of day (24 hour clock)</para>
        /// </summary>
        public short hour { get; set; }

        /// <summary>
        /// 数据记录的分
        /// <para>minute of hour</para>
        /// </summary>
        public short minute { get; set; }

        /// <summary>
        /// 数据记录的秒
        /// <para>second of minute</para>
        /// </summary>
        public short second { get; set; }

        /// <summary>
        /// 时间基准码
        /// <para>time basis code</para>
        /// </summary>
        public short timeBasisCode { get; set; }

        /// <summary>
        /// 道加权因子，最小有效位数定义为2伏
        /// <para>trace weighting factor, defined as 1/2^N volts for the least sigificant bit</para>
        /// </summary>
        public short traceWeightingFactor { get; set; }

        /// <summary>
        /// 滚动开关位置1的检波器组号
        /// <para>geophone group number of roll switch position one</para>
        /// </summary>
        public short grnors { get; set; }

        /// <summary>
        /// 野外原始记录中道号1的检波器组号
        /// <para>geophone group number of trace one within original field record</para>
        /// </summary>
        public short grnofr { get; set; }

        /// <summary>
        /// 野外原始记录中最后一道的检波器组号
        /// <para>geophone group number of last trace within original field record</para>
        /// </summary>
        public short grnlof { get; set; }

        /// <summary>
        /// 间隔大小（滚动时甩掉的总检波器组数）
        /// <para>gap size (total number of groups dropped)</para>
        /// </summary>
        public short gapSize { get; set; }

        /// <summary>
        /// 相对测线斜坡起始或终止点的移动
        /// <para>overtravel taper code</para>
        /// </summary>
        public short overtravelTaperCode { get; set; }


    }