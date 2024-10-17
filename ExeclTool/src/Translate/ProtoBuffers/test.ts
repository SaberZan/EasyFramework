import fs from "fs";
import { CfgSpace } from '../../../tmp/code/ts/MatchCfg'

let byts = fs.readFileSync("./tmp/bytes/MatchCfg.bytes");

let cfgs = CfgSpace.MatchCfgs.decode(byts);

console.log(JSON.stringify(cfgs));