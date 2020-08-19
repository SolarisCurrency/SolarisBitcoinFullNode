using System;
using System.Collections.Generic;
using System.Net;
using NBitcoin;
using NBitcoin.BouncyCastle.Math;
using NBitcoin.DataEncoders;
using NBitcoin.Protocol;
using Stratis.Bitcoin.Networks.Deployments;
using Stratis.Bitcoin.Networks.Policies;

namespace Stratis.Bitcoin.Networks
{
    public class SolarisMain : Network
    {
        /// <summary> The name of the root folder containing the different Solaris blockchains (SolarisMain, SolarisTest, SolarisRegTest). </summary>
        public const string SolarisRootFolderName = "solarisplatform";

        /// <summary> The default name used for the Solaris configuration file. </summary>
        public const string SolarisDefaultConfigFilename = "solaris.conf";

        public SolarisMain()
        {
            // The message start string is designed to be unlikely to occur in normal data.
            // The characters are rarely used upper ASCII, not valid as UTF-8, and produce
            // a large 4-byte int at any alignment.
            var messageStart = new byte[4];
            messageStart[0] = 0x71;
            messageStart[1] = 0x36;
            messageStart[2] = 0x23;
            messageStart[3] = 0x06;
            uint magic = BitConverter.ToUInt32(messageStart, 0); //0x6233671;

            this.Name = "SolarisMain";
            this.NetworkType = NetworkType.Mainnet;
            this.Magic = magic;
            this.DefaultPort = 60000;
            this.DefaultMaxOutboundConnections = 16;
            this.DefaultMaxInboundConnections = 109;
            this.DefaultRPCPort = 61000;
            this.DefaultAPIPort = 62000;
            this.MaxTipAge = 2 * 60 * 60;
            this.MinTxFee = 10000;
            this.FallbackFee = 10000;
            this.MinRelayTxFee = 10000;
            this.RootFolderName = SolarisRootFolderName;
            this.DefaultConfigFilename = SolarisDefaultConfigFilename;
            this.MaxTimeOffsetSeconds = 25 * 60;
            this.CoinTicker = "XLR";
            
            var consensusFactory = new PosConsensusFactory();

            // Create the genesis block.
            this.GenesisTime = 1572266171;
            this.GenesisNonce = 1834723;
            this.GenesisBits = 0x1e0fffff;
            this.GenesisVersion = 1;
            this.GenesisReward = Money.Zero;

            Block genesisBlock = CreateSolarisGenesisBlock(consensusFactory, this.GenesisTime, this.GenesisNonce, this.GenesisBits, this.GenesisVersion, this.GenesisReward);

            this.Genesis = genesisBlock;

            // Taken from SolarisX.
            var consensusOptions = new PosConsensusOptions(
                maxBlockBaseSize: 1_000_000,
                maxStandardVersion: 2,
                maxStandardTxWeight: 100_000,
                maxBlockSigopsCost: 20_000,
                maxStandardTxSigopsCost: 20_000 / 5
            );

            var buriedDeployments = new BuriedDeploymentsArray
            {
                [BuriedDeployments.BIP34] = 0,
                [BuriedDeployments.BIP65] = 0,
                [BuriedDeployments.BIP66] = 0
            };

            var bip9Deployments = new StratisBIP9Deployments
            {
                [StratisBIP9Deployments.ColdStaking] = new BIP9DeploymentsParameters(2, BIP9DeploymentsParameters.AlwaysActive, 999999999)
            };

            this.Consensus = new Consensus(
                consensusFactory: consensusFactory,
                consensusOptions: consensusOptions,
                coinType: 450,
                hashGenesisBlock: genesisBlock.GetHash(),
                subsidyHalvingInterval: 210000,
                majorityEnforceBlockUpgrade: 750,
                majorityRejectBlockOutdated: 950,
                majorityWindow: 1000,
                buriedDeployments: buriedDeployments,
                bip9Deployments: bip9Deployments,
                bip34Hash: new uint256("0x0000000000000000000000000000000000000000000000000000000000000000"),
                ruleChangeActivationThreshold: 1916, // 95% of 2016
                minerConfirmationWindow: 2016, // nPowTargetTimespan / nPowTargetSpacing
                maxReorgLength: 500,
                defaultAssumeValid: new uint256("0x0000000000000000000000000000000000000000000000000000000000000000"), // 0
                maxMoney: long.MaxValue,
                coinbaseMaturity: 50,
                premineHeight: 2,
                premineReward: Money.Coins(2032000),
                proofOfWorkReward: Money.Coins(0.25m),
                powTargetTimespan: TimeSpan.FromSeconds(14 * 24 * 60 * 60), // two weeks
                powTargetSpacing: TimeSpan.FromSeconds(10 * 60),
                powAllowMinDifficultyBlocks: false,
                posNoRetargeting: false,
                powNoRetargeting: false,
                powLimit: new Target(new uint256("00000fffffffffffffffffffffffffffffffffffffffffffffffffffffffffff")),
                minimumChainWork: null,
                isProofOfStake: true,
                lastPowBlock: 2500,
                proofOfStakeLimit: new BigInteger(uint256.Parse("00000fffffffffffffffffffffffffffffffffffffffffffffffffffffffffff").ToBytes(false)),
                proofOfStakeLimitV2: new BigInteger(uint256.Parse("000000000000ffffffffffffffffffffffffffffffffffffffffffffffffffff").ToBytes(false)),
                proofOfStakeReward: Money.Coins(0.25m)
            );

            this.Base58Prefixes = new byte[12][];
            this.Base58Prefixes[(int)Base58Type.PUBKEY_ADDRESS] = new byte[] { (75) };
            this.Base58Prefixes[(int)Base58Type.SCRIPT_ADDRESS] = new byte[] { (125) };
            this.Base58Prefixes[(int)Base58Type.SECRET_KEY] = new byte[] { (63 + 128) };
            this.Base58Prefixes[(int)Base58Type.ENCRYPTED_SECRET_KEY_NO_EC] = new byte[] { 0x01, 0x42 };
            this.Base58Prefixes[(int)Base58Type.ENCRYPTED_SECRET_KEY_EC] = new byte[] { 0x01, 0x43 };
            this.Base58Prefixes[(int)Base58Type.EXT_PUBLIC_KEY] = new byte[] { (0x04), (0x88), (0xB2), (0x1E) };
            this.Base58Prefixes[(int)Base58Type.EXT_SECRET_KEY] = new byte[] { (0x04), (0x88), (0xAD), (0xE4) };
            this.Base58Prefixes[(int)Base58Type.PASSPHRASE_CODE] = new byte[] { 0x2C, 0xE9, 0xB3, 0xE1, 0xFF, 0x39, 0xE2 };
            this.Base58Prefixes[(int)Base58Type.CONFIRMATION_CODE] = new byte[] { 0x64, 0x3B, 0xF6, 0xA8, 0x9A };
            this.Base58Prefixes[(int)Base58Type.STEALTH_ADDRESS] = new byte[] { 0x2a };
            this.Base58Prefixes[(int)Base58Type.ASSET_ID] = new byte[] { 23 };
            this.Base58Prefixes[(int)Base58Type.COLORED_ADDRESS] = new byte[] { 0x13 };

            this.Checkpoints = new Dictionary<int, CheckpointInfo>
            {
                { 0, new CheckpointInfo(new uint256("0xa3a98f72634c7d098164926b83ff136b66d1cafbb9aeb5a3b8d18da02937f79f"), new uint256("0x0000000000000000000000000000000000000000000000000000000000000000")) },
                { 1409, new CheckpointInfo(new uint256("0xd2f9c43c57fbb066daf940f80e9ce1a63d5d444e9e337b1491f79c36288ab0da"), new uint256("0x602e263081a44650085947dbe99fd0c51041389d79fb9c3f379f8a403a74d977")) },
                { 2900, new CheckpointInfo(new uint256("0x21e7bce830a7646e2ea049b7f37a42f032e09e8b2526cbf8d69fb094014826eb"), new uint256("0x83590685c1d2dfcb0ca9220af9447677a375cbfaaaf4305be309bb7141e41a47")) },
                { 191746, new CheckpointInfo(new uint256("0xf0120c783073c531cccb269d01029aaddd995efa4d8bad2546ec7b7c48f9415e"), new uint256("0xb6fe3b405b5016c94c29a98c2a90c83f70769c9c1718b157d44c6910099637a9")) }
            };

            this.Bech32Encoders = new Bech32Encoder[2];
            this.Bech32Encoders[(int)Bech32Type.WITNESS_PUBKEY_ADDRESS] = null;
            this.Bech32Encoders[(int)Bech32Type.WITNESS_SCRIPT_ADDRESS] = null;

            this.DNSSeeds = new List<DNSSeedData>
            {
                new DNSSeedData("node1.solarisdns.network", "node1.solarisdns.network"),
                new DNSSeedData("node2.solarisdns.network", "node2.solarisdns.network"),
                new DNSSeedData("node3.solarisdns.network", "node3.solarisdns.network"),
                new DNSSeedData("node4.solarisdns.network", "node4.solarisdns.network"),
                new DNSSeedData("node5.solarisdns.network", "node5.solarisdns.network")
            };

            this.SeedNodes = new List<NetworkAddress>
            {
                new NetworkAddress(IPAddress.Parse("3.125.89.141"), 60000), //Official node 1 (Frankfurt)
                new NetworkAddress(IPAddress.Parse("18.139.112.210"), 60000), //Official node 2 (Singapore)
                new NetworkAddress(IPAddress.Parse("18.211.206.174"), 60000), //Official node 3 (USA East)          
                new NetworkAddress(IPAddress.Parse("54.219.124.108"), 60000), //Official node 4 (USA West)
                new NetworkAddress(IPAddress.Parse("136.144.247.170"), 60000) //Official node 5 (The Netherlands)
            };

            this.StandardScriptsRegistry = new StratisStandardScriptsRegistry();
            
            Assert(this.Consensus.HashGenesisBlock == uint256.Parse("0xa3a98f72634c7d098164926b83ff136b66d1cafbb9aeb5a3b8d18da02937f79f"));
            Assert(this.Genesis.Header.HashMerkleRoot == uint256.Parse("0x64ebe67e26861a4608c4315a7cb5671e7d15fb7546989b2621bfb806bbc6ad08"));
        }

        protected static Block CreateSolarisGenesisBlock(ConsensusFactory consensusFactory, uint nTime, uint nNonce, uint nBits, int nVersion, Money genesisReward)
        {
            const string pszTimestamp = "https://www.solarisplatform.com";

            Transaction txNew = consensusFactory.CreateTransaction();
            txNew.Version = 1;
            txNew.Time = nTime;
            txNew.AddInput(new TxIn
            {
                ScriptSig = new Script(Op.GetPushOp(0), new Op
                {
                    Code = (OpcodeType)0x1,
                    PushData = new[] { (byte)42 }
                }, Op.GetPushOp(Encoders.ASCII.DecodeData(pszTimestamp)))
            });
            txNew.AddOutput(new TxOut()
            {
                Value = genesisReward,
            });

            Block genesis = consensusFactory.CreateBlock();
            genesis.Header.BlockTime = Utils.UnixTimeToDateTime(nTime);
            genesis.Header.Bits = nBits;
            genesis.Header.Nonce = nNonce;
            genesis.Header.Version = nVersion;
            genesis.Transactions.Add(txNew);
            genesis.Header.HashPrevBlock = uint256.Zero;
            genesis.UpdateMerkleRoot();
            return genesis;
        }
    }
}
