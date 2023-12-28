project功能 : 
建立醫院監測系統，醫院共有兩間病房，每間病房有兩個病床，每個病床有兩個感測器:beat和sugar，系統會根據感測器資料改變燈號，並上傳雲端


程式單元:
project共有4個單元 : 感測器、監測系統、node-red、thing speak
1. 感測器:在UI的左側，可以透過UI設定血糖、心律，並用HTTP上傳node-red(port1880)
2. node-red:有兩種flow。
	bed-flow:可以將來自感測器的HTTP request變成OM2M的格式，寫入mn resource tree。
	toBed-flow:可以將資料寫入in resource tree，並將資料HTTP給監測系統(port 6000)。
3. 監測系統:監聽port 6000，根據來自node-red toBed-flow的資料更新燈號(UI右側)，並上傳至thing speak。
4. thing speak:接受來自監測系統的資料，視覺化呈現。


執行步驟:
1. 當病患身上的感測器(app的左側)更新數值，程式會通知node-red的bed-flow
2. node-red bed-flow會更新mn-resource tree
3. mn下的sub會通知in AE，in AE的poa指向node-red的toBed-flow
4. toBed-flow會將資料寫到in resource tree。並通知監測系統(app的右側)
5. 監測系統(app的右側)會調整燈號。當病床的心跳異常會顯示藍燈，當病床的血糖異常會顯示紅燈，如果病房內的任一床有異常，病房會亮黃燈
6. 監測系統(app的右側)會將資料上傳至thing speak以視覺化呈現






