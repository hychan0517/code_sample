package com.unity3d.player

import android.app.Activity
import android.content.Context
import android.util.Log
import androidx.recyclerview.widget.RecyclerView
import com.kakao.sdk.auth.AuthApiClient
import com.kakao.sdk.auth.model.OAuthToken
import com.kakao.sdk.common.KakaoSdk
import com.kakao.sdk.common.model.KakaoSdkError
import com.kakao.sdk.common.util.Utility
import com.kakao.sdk.user.UserApiClient
import com.unity3d.player.UnityPlayer
import org.json.JSONException
import org.json.JSONObject

class KakaoAndroidPlugin : Activity() {

    private lateinit var viewAdapter: RecyclerView.Adapter<*>
    private lateinit var viewManager: RecyclerView.LayoutManager
    private var mIskakaosdkinit = false
    private var context: Context? = null
    private var activity: Activity? = null
    private var userID : String = ""

    fun SetContext(c: Context) {
        Log.d("UPlugin : ", "JAVA - Set Context")
        context = c
    }

    fun SetActivity(a: Activity) {
        Log.d("UPlugin : ", "JAVA - Set Context")
        activity = a
    }

    fun KakaoInit(callbackNum: Long) {
        //Init에 에러콜백이 없음..!
        if (mIskakaosdkinit == false) {
            mIskakaosdkinit = true

            //var keyHash = Utility.getKeyHash(context!!)
            //Log.d("UPlugin : ", keyHash)

            Log.d("UPlugin : ", "JAVA - KakaoSdkTest 초기화")
            KakaoSdk.init(context!!, "7461c3aaa5ffa2eeebcff19336adecdf")
            
            SendMessageFormat2(callbackNum, "KakaoInit", "Complete")
        } else {
            Log.d("UPlugin : ", "JAVA - 이미 KakaoSdk 초기화가 되어 있다")
            SendMessageFormat2(callbackNum, "KakaoInit", "Complete")
        }
    }

    fun IsSession(callbackNum: Long)
    {
        if (AuthApiClient.instance.hasToken()) {
            UserApiClient.instance.accessTokenInfo { tokenInfo, error ->
                if (error != null) {
                    if (error is KakaoSdkError && error.isInvalidTokenError()) {
                        //로그인 필요
                        Log.d("UPlugin : ", "JAVA - 로그인 필요")
                        SendMessageFormat2(callbackNum, "IsSession", "Closed")
                    }
                    else {
                        //기타 에러
                        Log.d("UPlugin : ", "JAVA - 기타 에러")
                        SendMessageFormat2(callbackNum, "Error", error.toString())
                    }
                }
                else {
                    //토큰 유효성 체크 성공(필요 시 토큰 갱신됨)
                    if (tokenInfo != null) {
                        Log.d("UPlugin : ", "JAVA - 토큰 유효성 체크 성공")
                        SendMessageFormat2(callbackNum, "IsSession", tokenInfo.id.toString())
                    }
                    else{
                        Log.d("UPlugin : ", "JAVA - 토큰 유효성 체크 성공했지만 토큰 없음")
                        SendMessageFormat2(callbackNum, "IsSession", "Closed")
                    }
                }
            }
        }
        else {
            //로그인 필요
            Log.d("UPlugin : ", "로그인 필요 토큰 없음")
            SendMessageFormat2(callbackNum, "IsSession", "Closed")
        }
    }

    fun Login(callbackNum: Long)
    {
        val callback: (OAuthToken?, Throwable?) -> Unit = { token, error ->
            if (error != null) {
                Log.e("UPlugin", "카카오계정으로 로그인 실패", error)
                SendMessageFormat2(callbackNum, "Error", error.toString())
            } else if (token != null) {
                Log.i("UPlugin", "카카오계정으로 로그인 성공 ${token.accessToken}")
                SendMessageFormat2(callbackNum, "OnSignupActivity", "onSuccess")
            }
        }

        UserApiClient.instance.loginWithKakaoAccount(context!!, callback = callback)
    }

    fun GetAccessToken(callbackNum: Long)
    {
        //IsSeeion과 동일하지만 v1 카카오 로직과 동일한 순서 맞추기위해 추가
        if (AuthApiClient.instance.hasToken()) {
            UserApiClient.instance.accessTokenInfo { tokenInfo, error ->
                if (error != null) {
                    if (error is KakaoSdkError && error.isInvalidTokenError()) {
                        //로그인 필요
                        Log.d("UPlugin : ", "JAVA - 로그인 필요")
                        SendMessageFormat2(callbackNum, "GetAccessToken", "null")
                    }
                    else {
                        //기타 에러
                        Log.d("UPlugin : ", "JAVA - 기타 에러")
                        SendMessageFormat2(callbackNum, "Error", error.toString())
                    }
                }
                else {
                    //토큰 유효성 체크 성공(필요 시 토큰 갱신됨)
                    if (tokenInfo != null) {
                        var tokenStr = String.format("%s:%s,%s:%s,%s:%s,%s:%s",
                            "AccessTokenToken", tokenInfo.id,
                            "RefreshToken", "",
                            "accessTokenExpiresAt",tokenInfo.expiresIn,
                            "refreshTokenExpiresAt", ""
                        )
                        Log.d("UPlugin : ", "JAVA - 토큰 유효성 체크 성공")
                        SendMessageFormat2(callbackNum, "GetAccessToken", tokenStr)
                    }
                    else{
                        Log.d("UPlugin : ", "JAVA - 토큰 유효성 체크 성공했지만 토큰 없음")
                        SendMessageFormat2(callbackNum, "GetAccessToken", "null")
                    }
                }
            }
        }
        else {
            //로그인 필요
            Log.d("UPlugin : ", "로그인 필요 토큰 없음")
            SendMessageFormat2(callbackNum, "GetAccessToken", "null")
        }
    }

    fun RequestMe(callbackNum: Long)
    {
        UserApiClient.instance.me { user, error ->
            if (error != null) {
                Log.e("UPlugin", "사용자 정보 요청 실패", error)
                SendMessageFormat2(callbackNum, "Error", error.toString())
            }
            else if (user != null) {
                Log.i(
                    "UPlugin", "사용자 정보 요청 성공" +
                            "\n회원번호: ${user.id}"
                )
                SendMessageFormat2(callbackNum, "RequestMe", "onSuccess")
                userID = user.id.toString()
            }
            else{
                SendMessageFormat2(callbackNum, "Error", "user is null")
            }
        }
    }

    fun GetUserID(callbackNum: Long)
    {
        SendMessageFormat2(callbackNum, "GetUserID", userID)
    }

    fun Logout(callbackNum: Long)
    {
        UserApiClient.instance.logout { error ->
            if (error != null) {
                Log.e("UPlugin", "로그아웃 실패. SDK에서 토큰 삭제됨", error)
                SendMessageFormat2(callbackNum,"Logout","onSessionClosed");
            }
            else {
                Log.i("UPlugin", "로그아웃 성공. SDK에서 토큰 삭제됨")
                SendMessageFormat2(callbackNum,"Logout","onSuccess");
            }
        }
    }

    @Throws(JSONException::class)
    private fun SendMessageFormat2(callbackNum: Long, keyName: String?, Value: String?) {
        val obj = JSONObject()
        obj.put("callbackNum", callbackNum)
        obj.put(keyName, Value)
        Log.d("UPlugin - JSONObject obj : ", obj.toString())
        UnityPlayer.UnitySendMessage("KakaoAndroidPlugin", "OnReceive", obj.toString())
    }
}