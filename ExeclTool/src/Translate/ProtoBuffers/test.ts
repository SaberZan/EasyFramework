import fs from "fs";
import path from "path";
import { EquipmentCfgData } from "../../../tmp/code/ts/EquipmentCfg_pb";

let byts = fs.readFileSync(path.join(__dirname, "../../../tmp/bytes/EquipmentCfg.bytes"));

let wrapper = EquipmentCfgData.deserializeBinary(byts);
let cfgs = wrapper.getDataList();
for (let cfg of cfgs) {
  console.log("id:", cfg.getId(), "name:", cfg.getName(), "price:", cfg.getPrice());
}
